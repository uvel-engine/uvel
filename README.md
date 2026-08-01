# Uvel Engine

<p align="center">
  <img src="assets/brand/uvel-logo.png" width="132" alt="Uvel Engine logo" />
</p>

<p align="center">
  <b>Simple XML. Native Windows UI. Fast desktop apps.</b><br/>
  A minimal UFlow framework for building modern WPF applications with hot reload, declarative logic and single-file EXE builds.
</p>

<p align="center">
  <a href="https://uflow.uz/uvel">Website</a> ·
  <a href="https://github.com/uvel-engine/uvel">GitHub</a> ·
  <a href="#quick-start">Quick Start</a> ·
  <a href="#features">Features</a>
</p>

---

## Why Uvel?

Uvel lets you describe desktop applications in readable XML instead of writing boilerplate XAML and repetitive C# for every screen. It is designed for quick tools, dashboards, internal apps, prototypes and lightweight Windows utilities.

## Features

- **XML-first UI** — build windows, layouts and components from a single `App.xml`.
- **Hot reload** — edit the XML and see UI/logic changes without restarting.
- **Declarative logic** — handlers, conditions, loops, HTTP calls, state updates and plugins from XML.
- **Built-in DevTools** — browser inspector for logs, state and runtime debugging.
- **Plugin system** — extend apps with C# plugins when XML is not enough.
- **Single EXE builds** — compile projects into standalone Windows executables.
- **UFlow-ready style** — clean, minimal and production-friendly defaults.

## Quick Start

```xml
<?xml version="1.0" encoding="utf-8"?>
<App Name="My Uvel App" Width="720" Height="420" Theme="Dark">
  <UI>
    <Grid Background="#101010">
      <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
        <TextBlock Text="Hello, Uvel!" FontSize="34" Foreground="White"/>
        <Button Content="Click me" onClick="OnClick"/>
        <TextBlock Name="status" Text="" Foreground="#888"/>
      </StackPanel>
    </Grid>
  </UI>
  <Logic>
    <Handler Name="OnClick">
      <Set Target="status" Property="Text" Value="Uvel is running."/>
      <Toast Message="Welcome to Uvel" Type="success"/>
    </Handler>
  </Logic>
</App>
```

Run it:

```bash
uvel run App.xml
uvel dev App.xml
uvel build App.xml --output ./dist --name MyApp
```

## Project Structure

```text
MyProject/
├── App.xml
├── uvel.json
├── pages/
├── components/
├── assets/
└── plugins/
```

## Build from Source

```bash
git clone https://github.com/uvel-engine/uvel.git
cd uvel/engine
build.bat
```

The built binary is `engine/bin/uvel.exe`.

## License

MIT — see [LICENSE](LICENSE).

<p align="center">Made with UFlow · https://uflow.uz/uvel</p>

## Uvel Workspace Bridge

`uflow.uz/uvel` includes a browser-based Uvel Workspace editor. Browsers cannot launch `.exe` files directly, so Uvel provides a local Windows bridge.

```bash
uvel install-protocol
uvel bridge --port 9327
```

Then open:

```text
https://uflow.uz/uvel#workspace
```

The Workspace connects to:

```text
ws://127.0.0.1:9327/workspace
```

Supported realtime actions:

- **Run XML** — saves the XML to the local workspace and launches Uvel in dev mode.
- **Hot reload** — rewriting the XML file triggers Uvel's file watcher and updates the running window.
- **Hot restart** — stops the running Uvel process and starts it again with the latest XML.
- **Realtime logs** — bridge logs and app stdout/stderr stream back to the browser.

The bridge listens only on localhost and runs only after the user explicitly starts it.
