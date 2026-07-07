const WebSocket = require('ws');
const PORT = 8080;
const wss = new WebSocket.Server({ port: PORT });
console.log("INIT: Signaling server en ws://localhost:8080");

// Ahora soportamos varias sesiones de Unity en paralelo.
// Cada sesión se identifica con un id numérico de 4 dígitos (ej: "1234"),
// que Unity debe mandar como query param al conectarse: ?type=unity&id=1234

const unityClients = new Map();       // sessionId -> ws de Unity
const browserClients = new Map();     // sessionId -> Map<clientId, ws>
const pendingForUnity = new Map();    // sessionId -> array de mensajes pendientes
let nextClientId = 0;                 // contador global de clientes navegador

const SESSION_ID_REGEX = /^\d{4}$/;

function getOrCreateBrowserMap(sessionId) {
    if (!browserClients.has(sessionId)) {
        browserClients.set(sessionId, new Map());
    }
    return browserClients.get(sessionId);
}

function getPendingQueue(sessionId) {
    if (!pendingForUnity.has(sessionId)) {
        pendingForUnity.set(sessionId, []);
    }
    return pendingForUnity.get(sessionId);
}

wss.on('connection', (ws, req) => {
    const params = new URL(req.url, 'http://localhost').searchParams;
    const clientType = params.get('type');
    const sessionId = params.get('id');

    if (clientType === 'unity') {
        // Validar el id de sesión
        if (!sessionId || !SESSION_ID_REGEX.test(sessionId)) {
            console.log(`ERR: Unity conectado con id inválido ("${sessionId}") — se esperaba un número de 4 dígitos`);
            ws.close(1008, 'ID de sesión inválido, debe ser numérico de 4 dígitos');
            return;
        }

        if (unityClients.has(sessionId)) {
            console.log(`ERR: Unity con id=${sessionId} ya está conectado`);
            ws.close(1008, 'ID de sesión ya en uso');
            return;
        }

        unityClients.set(sessionId, ws);
        console.log(`CONN: Unity conectado (session=${sessionId})`);

        // Enviar mensajes pendientes de esta sesión
        const pending = getPendingQueue(sessionId);
        for (const data of pending) ws.send(data);
        pendingForUnity.set(sessionId, []);

        ws.on('message', (data) => {
            const msg = JSON.parse(data);
            console.log(`MSSG: [unity ${sessionId}] → ${msg.type}`);

            const sessionBrowsers = browserClients.get(sessionId);
            if (!sessionBrowsers) return;

            if (msg.clientId !== undefined) {
                // Mensaje dirigido a un cliente concreto de esta sesión
                const target = sessionBrowsers.get(msg.clientId);
                if (target?.readyState === WebSocket.OPEN) {
                    target.send(data);
                }
            } else {
                // Broadcast a todos los móviles de esta sesión (offer e ICE iniciales)
                for (const [, client] of sessionBrowsers) {
                    if (client.readyState === WebSocket.OPEN) {
                        client.send(data);
                    }
                }
            }
        });

        ws.on('close', () => {
            console.log(`CLSE: Unity desconectado (session=${sessionId})`);
            unityClients.delete(sessionId);
            pendingForUnity.delete(sessionId);

            // Avisar y limpiar a los navegadores huérfanos de esta sesión
            const sessionBrowsers = browserClients.get(sessionId);
            if (sessionBrowsers) {
                for (const [, client] of sessionBrowsers) {
                    if (client.readyState === WebSocket.OPEN) {
                        client.close(1001, 'Sesión de Unity finalizada');
                    }
                }
            }
            browserClients.delete(sessionId);
        });

    } else {
        // Cliente navegador: debe indicar a qué sesión de Unity quiere conectarse
        if (!sessionId || !SESSION_ID_REGEX.test(sessionId)) {
            console.log(`ERR: Navegador conectado sin id de sesión válido ("${sessionId}")`);
            ws.close(1008, 'Debes indicar ?id=XXXX con el id de sesión de Unity');
            return;
        }

        const clientId = nextClientId++;
        const sessionBrowsers = getOrCreateBrowserMap(sessionId);
        sessionBrowsers.set(clientId, ws);
        console.log(`CONN: Navegador conectado (session=${sessionId}, id=${clientId})`);

        // Avisar a Unity (si esa sesión está conectada) que hay un cliente nuevo
        const unityClient = unityClients.get(sessionId);
        if (unityClient?.readyState === WebSocket.OPEN) {
            unityClient.send(JSON.stringify({ type: 99, clientId }));
        }

        ws.on('message', (data) => {
            const msg = JSON.parse(data);
            console.log(`MSSG: [browser ${sessionId}/${clientId}] → ${msg.type}`);

            // Añadir clientId para que Unity sepa de qué móvil viene
            const tagged = JSON.stringify({ ...msg, clientId });

            const unity = unityClients.get(sessionId);
            if (unity?.readyState === WebSocket.OPEN) {
                unity.send(tagged);
            } else {
                getPendingQueue(sessionId).push(tagged);
            }
        });

        ws.on('close', () => {
            console.log(`CLSE: Navegador ${clientId} desconectado (session=${sessionId})`);
            sessionBrowsers.delete(clientId);

            const unity = unityClients.get(sessionId);
            if (unity?.readyState === WebSocket.OPEN) {
                unity.send(JSON.stringify({ type: 4, clientId })); // 4 = ConnectionEvent.DISCONNECT
            }
        });
    }
});
