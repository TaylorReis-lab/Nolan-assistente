# ⚡ Jarvis Assistant

Assistente pessoal estilo Jarvis para Windows, desenvolvido em .NET MAUI (C#).

(Será feita uma versão para Android/iOS no futuro, mas o foco inicial é Windows), como sou desenvolvedor C# e quero aproveitar o poder do .NET MAUI para criar uma aplicação nativa, leve e rápida.
também sera possível rodar no linux e macOS, mas o foco inicial é Windows, para aproveitar as APIs nativas de automação e voz, como sou somente um desenvolvedor irá ser feito aos poucos, desculpe se ouver bugs ou falta de recursos, mas estou fazendo o meu melhor para entregar uma experiência incrível!
projeto inspirado no Jarvis do filme Homem de Ferro, com uma interface futurista e funcionalidades avançadas de IA, automação e integração com serviços web.
e podem aver troca de nome, caso alguém tiver uma sugestão melhor, estou aberto a ideias!

---

## 🗂️ Estrutura do Projeto

```
JarvisAssistant/
├── Core/
│   └── JarvisCore.cs          ← Orquestrador central (IA + Voz + Automação + Web)
├── Models/
│   ├── JarvisConfig.cs        ← Modelo de configuração
│   └── ChatMessage.cs         ← Modelo de mensagem
├── Services/
│   ├── AIService.cs           ← IA: Claude / OpenAI / Ollama
│   ├── VoiceService.cs        ← Voz: STT + TTS (Windows nativo ou Azure)
│   ├── AutomationService.cs   ← Automação do Windows
│   ├── WebService.cs          ← Busca web, clima, notícias
│   └── ConfigurationService.cs← Persistência de configurações (JSON)
├── UI/
│   ├── MainPage.xaml(.cs)     ← Tela principal de chat
│   └── SetupPage.xaml(.cs)    ← Wizard de configuração inicial
├── MauiProgram.cs             ← Startup + DI
└── App.xaml.cs                ← Shell + roteamento
```

---

## 🚀 Como Rodar

### Pré-requisitos
- **Visual Studio 2022** (versão 17.8+) com workload **.NET MAUI**
- **.NET 8 SDK**
- **Windows 10 versão 1903** ou superior

### Passo a passo

1. **Clone ou abra o projeto** no Visual Studio 2022.

2. **Restaure os pacotes NuGet:**
   ```
   dotnet restore
   ```

3. **Selecione o target:** `net8.0-windows10.0.19041.0`

4. **Pressione F5** para rodar.

Na primeira execução, o wizard de configuração será aberto automaticamente.

---

## 🔑 APIs e Chaves

### IA (obrigatório — escolha um)

| Provider | Onde obter | Custo |
|----------|-----------|-------|
| **Claude (Anthropic)** | console.anthropic.com | Pago (melhor qualidade) |
| **OpenAI (GPT-4o)** | platform.openai.com | Pago |
| **Ollama** | ollama.com | **Gratuito** (roda local) |

#### Configurar Ollama (opção gratuita):
```bash
# 1. Baixe e instale: https://ollama.com
# 2. Baixe um modelo:
ollama pull llama3
# ou
ollama pull mistral
# 3. Deixe rodando (ele fica em http://localhost:11434)
```

---

### Voz (opcional)

**Windows TTS** (nativo): funciona sem configuração.

**Azure Cognitive Speech** (qualidade superior):
1. Acesse: portal.azure.com
2. Crie um recurso **Speech Services** (tier gratuito F0 disponível)
3. Copie a **Key** e a **Region**

---

### Web (opcional)

| Serviço | Onde obter | Tier gratuito |
|---------|-----------|--------------|
| OpenWeatherMap (clima) | openweathermap.org/api | ✅ 60 calls/min |
| NewsAPI (notícias) | newsapi.org | ✅ 100 requests/day |
| DuckDuckGo (busca) | — | ✅ Sem chave |

---

## 🎤 Comandos de Voz

Fale **"Jarvis"** seguido do comando:

| Exemplo | O que faz |
|---------|-----------|
| `"Jarvis, abrir Chrome"` | Abre o Chrome |
| `"Jarvis, que horas são?"` | Fala hora atual |
| `"Jarvis, qual o clima em Brasília?"` | Busca clima |
| `"Jarvis, notícias de hoje"` | Lê manchetes |
| `"Jarvis, aumentar volume"` | Aumenta volume do sistema |
| `"Jarvis, bloquear computador"` | Bloqueia a tela |

---

## ⚙️ Configurações

Ficam salvas em:
```
%AppData%\JarvisAssistant\appsettings.json
```

Você pode editar o arquivo diretamente ou usar a tela de configurações dentro do app (ícone ⚙️).

---

## 🏗️ Próximos Passos Sugeridos

- [ ] Interface animada estilo HUD (gradientes, partículas)
- [ ] Integração com agenda (Google Calendar / Outlook)
- [ ] Lembrete e alarmes
- [ ] Controle de smart home (Home Assistant)
- [ ] Plugin system para extensibilidade
- [ ] Histórico de conversas persistido
- [ ] Hotkey global (ativa sem abrir a janela)
