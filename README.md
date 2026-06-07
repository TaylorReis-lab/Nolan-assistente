# ⚡ Nolan Assistant

> Assistente pessoal para Windows inspirado no Jarvis do Homem de Ferro.
> Desenvolvido em C# com foco em automação local, leveza e extensibilidade.

---

> [!WARNING]
> **Projeto em refatoração ativa.**
> A arquitetura está sendo reescrita do zero para ficar mais sólida antes de crescer.
> Bugs são esperados, recursos estão incompletos — isso é intencional por enquanto.
> Acompanhe o progresso pelas issues e commits.

---

## 🗺️ Visão Geral

O Nolan é um assistente pessoal de desktop que você controla por texto ou voz.
O foco inicial é **Windows** — aproveitando as APIs nativas de automação e voz do .NET —
mas a base em .NET MAUI permitirá portar para Android, iOS, Linux e macOS no futuro.

Funcionalidades planejadas (nem todas prontas ainda):

- Automação local: abrir/fechar apps, controlar volume, bloquear tela
- Modos de trabalho: ativar um conjunto de apps com uma frase
- IA conversacional: Claude, OpenAI ou Ollama (local e gratuito)
- Reconhecimento e síntese de voz
- Integração com web: clima, notícias, busca

---

## 🏗️ Estado atual do projeto

| Módulo | Status |
|---|---|
| CLI de automação local | ✅ Funcional |
| Modos de trabalho (criar/ativar/remover) | ✅ Funcional |
| Abertura e fechamento de apps | ✅ Funcional |
| Comandos de sistema (hora, volume, lock) | ✅ Funcional |
| Interface MAUI (desktop) | 🔄 Em refatoração |
| Reconhecimento de voz | 🔄 Em refatoração |
| Integração com IA | ⏳ Planejado |
| Integração web (clima, notícias) | ⏳ Planejado |
| Hotkey global | ⏳ Planejado |

---

## 🗂️ Estrutura atual

```
JarvisCLI/                      ← versão CLI (testável agora)
├── Program.cs                  ← entry point
├── Core/
│   ├── Dispatcher.cs           ← roteia input para o comando certo
│   └── UI.cs                   ← output colorido centralizado
├── Commands/
│   ├── ICommand.cs             ← interface base de todo comando
│   ├── ModeCommand.cs          ← gerencia modos de trabalho
│   ├── AppCommand.cs           ← abre e fecha apps
│   ├── SystemCommand.cs        ← hora, volume, lock, desligar
│   └── HelpCommand.cs          ← ajuda
├── Services/
│   ├── ModeStore.cs            ← persiste modos em JSON
│   └── Launcher.cs             ← executa processos do Windows
└── Models/
    ├── WorkMode.cs             ← modelo de modo de trabalho
    └── CommandResult.cs        ← tipo universal de retorno
```

A versão MAUI (interface gráfica) está sendo refatorada separadamente e será integrada quando a base CLI estiver estável.

---

## 🚀 Como rodar (CLI)

### Pré-requisitos

- **.NET 8 SDK** — [download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Windows 10 1903** ou superior

### Passos

```powershell
# Entre na pasta do projeto CLI
cd JarvisCLI

# Restaure os pacotes
dotnet restore

# Execute
dotnet run
```

Na primeira execução, três modos de exemplo são criados automaticamente em `%AppData%\JarvisCLI\modes.json`.

---

## 💬 Comandos disponíveis

### Modos de trabalho

```
modo                            → lista todos os modos
modo ativar hora de trabalhar   → ativa o modo pelo gatilho
modo criar                      → assistente interativo para criar novo modo
modo remover "Foco Total"       → remove um modo
```

Ou ative diretamente pelo gatilho, sem prefixo:

```
hora de trabalhar
foco total
hora de descansar
```

### Apps

```
abrir chrome
abrir "VS Code"
fechar discord
abrir https://youtube.com
```

### Sistema

```
hora              → mostra hora e data atual
bloquear          → bloqueia a tela
volume +          → aumenta o volume
volume -          → diminui o volume
volume mudo       → muta/desmuta
desligar 60       → desliga em 60 segundos
reiniciar         → reinicia o computador
```

### Geral

```
ajuda             → lista todos os comandos
sair              → fecha o Nolan
```

---

## ⚙️ Configuração de modos

Os modos ficam salvos em:

```
%AppData%\JarvisCLI\modes.json
```

Você pode editar o arquivo diretamente ou usar o comando `modo criar` no CLI.

Exemplo de modo no JSON:

```json
{
  "name": "Hora de Trabalhar",
  "trigger": "hora de trabalhar",
  "open": [
    { "label": "VS Code",  "executable": "code"    },
    { "label": "Discord",  "executable": "discord" },
    { "label": "Spotify",  "executable": "spotify" }
  ],
  "close": [
    { "label": "WhatsApp", "executable": "whatsapp" }
  ]
}
```

---

## 🔑 APIs futuras (quando IA for integrada)

| Provider | Onde obter | Custo |
|---|---|---|
| **Claude (Anthropic)** | console.anthropic.com | Pago |
| **OpenAI (GPT-4o)** | platform.openai.com | Pago |
| **Ollama** | ollama.com | **Gratuito** (roda local) |

Para usar Ollama sem custo:

```bash
# 1. Instale: https://ollama.com
# 2. Baixe um modelo:
ollama pull llama3
# 3. Deixe rodando (http://localhost:11434)
```

---

## 🛣️ Roadmap

### Fase 1 — Base CLI (em andamento)
- [x] Estrutura de comandos extensível (`ICommand`)
- [x] Sistema de modos com persistência JSON
- [x] Automação de apps (abrir/fechar)
- [x] Comandos de sistema (volume, lock, hora)
- [ ] Testes unitários dos serviços core

### Fase 2 — Interface gráfica
- [ ] Refatoração da UI MAUI
- [ ] Tela de modos com editor visual
- [ ] Painel de estatísticas
- [ ] Tema futurista finalizado

### Fase 3 — IA e voz
- [ ] Integração com Claude / OpenAI / Ollama
- [ ] Reconhecimento de voz (wake word)
- [ ] Síntese de voz (TTS)
- [ ] Memória de contexto entre sessões

### Fase 4 — Integrações
- [ ] Hotkey global (ativa sem abrir a janela)
- [ ] Clima e notícias
- [ ] Google Calendar / Outlook
- [ ] Plugin system para extensibilidade
- [ ] Android / iOS / Linux / macOS

---

## 🤝 Contribuindo

Projeto desenvolvido por uma pessoa só, aos poucos, com carinho.
Se encontrar um bug ou tiver uma ideia, abra uma issue — toda sugestão é bem-vinda.

> O nome "Nolan" ainda pode mudar. Se tiver uma sugestão melhor, manda ver!

---

## 📄 Licença

MIT — use, modifique e distribua à vontade.