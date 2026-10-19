using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Telas da Interface")]
    public GameObject screenMenu;
    public GameObject screenPareamento;
    public GameObject screenConfig;
    public GameObject screenCreditos;

    void Start()
    {
        // Garante que o jogo sempre inicie no menu principal
        AbrirMenuPrincipal();
    }

    private void EsconderTodasAsTelas()
    {
        if (screenMenu != null) screenMenu.SetActive(false);
        if (screenPareamento != null) screenPareamento.SetActive(false);
        if (screenConfig != null) screenConfig.SetActive(false);
        if (screenCreditos != null) screenCreditos.SetActive(false);
    }

    public void AbrirMenuPrincipal()
    {
        EsconderTodasAsTelas();
        screenMenu.SetActive(true);
    }

    public void AbrirPareamento()
    {
        EsconderTodasAsTelas();
        screenPareamento.SetActive(true);
    }

    public void AbrirConfiguracoes()
    {
        EsconderTodasAsTelas();
        screenConfig.SetActive(true);
    }

    public void AbrirCreditos()
    {
        EsconderTodasAsTelas();
        screenCreditos.SetActive(true);
    }

    public void SairDaAplicacao()
    {
        Debug.Log("Encerrando Didascálias VR...");
        Application.Quit();
    }
}