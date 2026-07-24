# Azure Speech SDK for .NET

Azure Speech is a cloud service that provides speech-to-text, text-to-speech, speech translation, and other speech-related capabilities. The Speech SDK allows you to integrate real-time speech recognition, speech synthesis, translation, and advanced speech analytics into your applications.

The Azure Speech SDK for .NET provides APIs to perform speech-to-text transcription, text-to-speech synthesis, speech translation, language identification, and pronunciation assessment. Build applications that can understand spoken language, generate natural-sounding speech, and enable multilingual communication.

[Source code][speech_sdk_src] | [Package (NuGet)][speech_sdk_nuget] | [API reference documentation][speech_sdk_api_ref] | [Product documentation][speech_docs] | [Samples][speech_samples] | [Quickstart guides][speech_quickstart]

## Getting started

### Install the package

Install the Azure Speech SDK for .NET with [NuGet][nuget]:

```dotnetcli
dotnet add package Microsoft.CognitiveServices.Speech
```

### Prerequisites

* An [Azure subscription][azure_sub]
* A [Speech resource][create_speech_resource] created in the Azure portal
* An understanding of [Speech service regions and endpoints][speech_regions]
* Appropriate Azure credentials configured (DefaultAzureCredential, Managed Identity, Azure CLI, etc.)

If you use the Azure CLI, replace `<your-resource-group-name>` and `<your-speech-resource-name>` with your own, unique names:

```bash
az cognitiveservices account create --name <your-speech-resource-name> --resource-group <your-resource-group-name> --kind SpeechServices --sku F0 --location eastus
```

### Authenticate and configure the client

To interact with the Speech service, you'll need to create an instance of the appropriate client class (such as `SpeechRecognizer`, `SpeechSynthesizer`, or `TranslationRecognizer`). You can authenticate using Azure credentials and connect using your Speech service **endpoint**.

```csharp
using Microsoft.CognitiveServices.Speech;
using Azure.Identity;

// Using DefaultAzureCredential for authentication
string endpoint = "https://YOUR_REGION.api.cognitive.microsoft.com/"; // e.g., "https://eastus.api.cognitive.microsoft.com/"
var credential = new DefaultAzureCredential();

var speechConfig = SpeechConfig.FromEndpoint(new Uri(endpoint), credential);
speechConfig.SpeechRecognitionLanguage = "en-US";
```

You can also use other Azure credential types:

```csharp
using Microsoft.CognitiveServices.Speech;
using Azure.Identity;

string endpoint = "https://YOUR_REGION.api.cognitive.microsoft.com/";

// Using Managed Identity
var managedIdentityCredential = new ManagedIdentityCredential();
var speechConfig = SpeechConfig.FromEndpoint(new Uri(endpoint), managedIdentityCredential);

// Using Azure CLI credential (for development)
var azureCliCredential = new AzureCliCredential();
var speechConfigCli = SpeechConfig.FromEndpoint(new Uri(endpoint), azureCliCredential);

// Using Client Secret credential
var clientSecretCredential = new ClientSecretCredential("tenantId", "clientId", "clientSecret");
var speechConfigSecret = SpeechConfig.FromEndpoint(new Uri(endpoint), clientSecretCredential);
```

## Key concepts

### SpeechConfig

A `SpeechConfig` object contains the configuration information needed to use the Speech service. It includes your subscription key, region, and other settings like language and audio format.

### SpeechRecognizer

A `SpeechRecognizer` enables speech-to-text capabilities. It can transcribe speech from microphones, audio files, or audio streams in real-time.

### SpeechSynthesizer

A `SpeechSynthesizer` enables text-to-speech capabilities. It can convert text to natural-sounding speech and save to files or play through speakers.

### TranslationRecognizer

A `TranslationRecognizer` provides speech translation capabilities, allowing you to translate spoken language in real-time.

### Thread safety

All client instance methods are thread-safe and independent of each other. This ensures that you can safely reuse client instances across threads.

### Additional concepts

[Speech service pricing][speech_pricing] |
[Supported languages][speech_languages] |
[Speech containers][speech_containers] |
[Custom models][custom_speech] |
[Long-running operations][long_running_operations] |
[Error handling and diagnostics][speech_diagnostics]

## Examples and samples

The Azure Speech SDK supports synchronous and asynchronous APIs for all major operations. Instead of maintaining code samples directly in this README, we recommend exploring our comprehensive sample repositories:

### Quick start samples

Get started quickly with these basic scenarios:

* **[Speech-to-text quickstart][stt_quickstart]** - Recognize speech from microphone or audio files
* **[Text-to-speech quickstart][tts_quickstart]** - Convert text to natural-sounding speech  
* **[Speech translation quickstart][translation_quickstart]** - Translate spoken language in real-time

### Comprehensive sample repository

For in-depth examples covering all Speech SDK capabilities, visit our **[Speech SDK samples repository][speech_samples]** on GitHub. This repository includes:

**Speech-to-text scenarios:**
- Microphone recognition
- File transcription  
- Continuous recognition
- Batch transcription
- Custom speech models

**Text-to-speech scenarios:**
- Basic speech synthesis
- SSML markup usage
- Custom voices
- Audio output options

**Advanced scenarios:**
- Speech translation
- Pronunciation assessment
- Language identification

**Platform-specific samples:**
- Console applications
- UWP applications
- WebAssembly scenarios

### Try Speech Studio

For no-code experimentation and prototyping, try **[Speech Studio][speech_studio]** - our web-based interface for testing Speech services without writing any code.

## Migration and version information

If you're migrating from an older version of the Speech SDK, see our [migration guides][migration_guide] for detailed information about breaking changes and new features.

## Troubleshooting

### Common issues

* **Authentication errors**: Ensure your Azure credentials are properly configured and have access to the Speech resource
* **Endpoint configuration**: Verify the Speech service endpoint URL matches your resource's region
* **Audio input problems**: Check microphone permissions and audio device settings  
* **Network connectivity**: Verify internet connection and firewall settings
* **Language support**: Confirm your desired language is [supported][speech_languages]

### Enable logging

For details on enabling SDK logging and diagnostic options for C#, see the documentation: [Enable and use logging in the Speech SDK (C#)](https://learn.microsoft.com/azure/ai-services/speech-service/how-to-use-logging?pivots=programming-language-csharp)

For more troubleshooting information, see the [troubleshooting guide][troubleshooting].

## Next steps

* Explore [Speech Studio][speech_studio] for testing and prototyping
* Try [custom speech models][custom_speech] for domain-specific accuracy
* Learn about [Speech containers][speech_containers] for on-premises deployment
* Review [best practices][best_practices] for production applications

## Contributing

This project welcomes contributions and suggestions. See our [contributing guidelines][contributing] for more information.



[speech_sdk_src]: https://github.com/Azure-Samples/cognitive-services-speech-sdk
[speech_sdk_nuget]: https://www.nuget.org/packages/Microsoft.CognitiveServices.Speech
[speech_sdk_api_ref]: https://docs.microsoft.com/dotnet/api/microsoft.cognitiveservices.speech
[speech_docs]: https://docs.microsoft.com/azure/cognitive-services/speech-service/
[speech_samples]: https://github.com/Azure-Samples/cognitive-services-speech-sdk
[speech_quickstart]: https://docs.microsoft.com/azure/cognitive-services/speech-service/get-started-speech-to-text
[stt_quickstart]: https://docs.microsoft.com/azure/cognitive-services/speech-service/get-started-speech-to-text
[tts_quickstart]: https://docs.microsoft.com/azure/cognitive-services/speech-service/get-started-text-to-speech
[translation_quickstart]: https://docs.microsoft.com/azure/cognitive-services/speech-service/get-started-speech-translation
[nuget]: https://www.nuget.org/
[azure_sub]: https://azure.microsoft.com/free/
[create_speech_resource]: https://docs.microsoft.com/azure/cognitive-services/speech-service/overview#create-a-speech-resource-in-azure
[speech_regions]: https://docs.microsoft.com/azure/cognitive-services/speech-service/regions
[speech_pricing]: https://azure.microsoft.com/pricing/details/cognitive-services/speech-services/
[speech_languages]: https://docs.microsoft.com/azure/cognitive-services/speech-service/language-support
[speech_containers]: https://docs.microsoft.com/azure/cognitive-services/speech-service/speech-container-overview
[custom_speech]: https://docs.microsoft.com/azure/cognitive-services/speech-service/custom-speech-overview
[long_running_operations]: https://docs.microsoft.com/azure/cognitive-services/speech-service/batch-transcription
[speech_diagnostics]: https://docs.microsoft.com/azure/cognitive-services/speech-service/how-to-use-logging
[migration_guide]: https://docs.microsoft.com/azure/cognitive-services/speech-service/migration-guides
[troubleshooting]: https://docs.microsoft.com/azure/cognitive-services/speech-service/troubleshooting
[speech_studio]: https://speech.microsoft.com/
[best_practices]: https://docs.microsoft.com/azure/cognitive-services/speech-service/best-practices
[contributing]: https://github.com/Azure-Samples/cognitive-services-speech-sdk/blob/master/CONTRIBUTING.md
