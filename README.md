# Blazor Data Hydration: demo app

This is the demo code for the Pluralsight course **Data Hydration & State Persistence in ASP.NET Core 10 Blazor**.

It's a small blogging app, built SSR-first. Every page is rendered on the server first, and individual components then spin up their interactivity, either on the server (a SignalR circuit) or in the browser (WebAssembly). The course uses it to show what happens to your data during that hand-over, and how to make sure it arrives intact.

## What you need

- The [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2026, VS Code with the C# Dev Kit, or Rider
- A Chromium-based browser or Firefox. Most of the interesting detail is in the browser's DevTools.

## Running the app

From the repository root:

```bash
dotnet run --project BlazorBloggingApp --launch-profile https
```

Then browse to <https://localhost:7144>. The `http` profile serves <http://localhost:5200> instead. In Visual Studio, open `BlazorHydration.slnx` and press F5.

The posts are held in memory, so any edits are lost when the app restarts.

## Running the tests

```bash
dotnet test
```

The test project uses xUnit and bUnit. The tests in `LearningOutcomes/` pin the claims the course makes about the app, and they're tagged by learning objective and clip, so you can run just one slice:

```bash
dotnet test --filter "LO=TO2"
dotnet test --filter "Clip=M4C1"
```

## Following the course

The commits follow the course, and the tags mark the points the demos start and finish. Check out a tag to see the code exactly as it is at that point:

```bash
git checkout m2-start
```

| Tag | Where it fits | What the code looks like |
|---|---|---|
| `baseline` | Before Module 1 | The SSR-first blog. The layout is static, the post list is an interactive server island, and the post page runs in `InteractiveAuto`. |
| `m1-end` | End of Module 1 | The hand-over is slowed down so you can watch it. The post is fetched twice, once for the SSR load and again when the interactivity spins up. |
| `m2-start` | Start of Module 2 | Same commit as `m1-end`. |
| `m2c4-start` | During Module 2 | The post list persists its posts with `[PersistentState]`. The post page gets its render bar, and the server log names the caller of every fetch. |
| `m2-end` | End of Module 2 | The post page runs in `InteractiveWebAssembly` and persists its post as well, so neither component fetches again after the hand-over. |
| `m4-start` | Start of Module 4 | The theme is shared through a **singleton** `AppState`, which leaks one user's theme to everyone. |
| `m4c1-end` | Module 4, after the trap | `AppState` is scoped, so there's no leak, but the theme no longer has anywhere to live. |
| `m4c2` | Module 4, giving the theme a home | The theme lives in a cookie, and each request copies it into `AppState`. |
| `m4c3-start` | Module 4, handing over `AppState` | The post page reads `AppState`, and the card flips back to the light theme when WebAssembly spins up. |
| `m4c3-end` | Module 4, `AppState` handed over | `AppState` is registered as a persistent service, so WebAssembly starts with the theme from the SSR load. |
| `m4-end` | End of Module 4 | The learning-outcome tests for Module 4. |
| `m5` | End of the course | A bonus search filter on the post list, held in the URL. `main` is the finished app. |

Module 3 has no code changes, so it has no tags. It works with the code as it is at `m2-end`.

## What's in the app

- **Home page (`/`):** a static welcome page.
- **Post list (sidebar):** an `InteractiveServer` island in the static layout. It holds its posts in persisted state, sorts them newest or oldest first, and filters them by title. The filter is held in the URL (`/?q=blazor`), so it survives a reload and the SSR load renders the filtered list.
- **Post page (`/post/{id}`):** runs in `InteractiveWebAssembly` from the `BlazorBloggingApp.Client` project. Its post is persisted across the hand-over. The like button stores its count in the browser's local storage, so it only appears once the interactivity has spun up.
- **Theme toggle:** a plain form post that sets a `theme` cookie. Each request copies the cookie into a scoped `AppState`, which is handed to WebAssembly as a persistent service.
- **Edit page (`/edit-post/{id}`):** an `InteractiveServer` form for updating a post. Everyone is signed in as a demo admin user.

## Seeing the hand-over

The app has a few diagnostic aids that you wouldn't put in a real app.

- **The render bar** on each post page shows `RendererInfo.Name`, where the render runs, whether it's interactive, and the current theme. Watch it change from `Static` to `WebAssembly` as the page loads.
- **The server log** prints a line for every fetch, with a guess at who asked for it:

  ```text
  Fetching post 11111111-1111-1111-1111-111111111111 [SSR full load /post/11111111-1111-1111-1111-111111111111]
  Fetching post 11111111-1111-1111-1111-111111111111 [WASM via HTTP GET /api/posts/11111111-1111-1111-1111-111111111111]
  ```

- **Two simulated delays:** Blazor waits three seconds before it starts the interactivity, so the SSR load stays on screen long enough to look at. Any post fetch made after the interactivity has spun up then waits for two seconds, so the extra fetch is easy to see.
- **The state comments:** view the page source and scroll to the bottom. Persisted state travels in HTML comments. `Blazor-WebAssembly-Component-State` is base64-encoded JSON that you can decode and read, and `Blazor-Server-Component-State` is encrypted.

## Project layout

```text
BlazorBloggingApp/          The server: host, layout, static pages, the post list, the in-memory data and the API
BlazorBloggingApp.Client/   The WebAssembly side: the post page, AppState and the HTTP API client
BlazorBloggingApp.Tests/    xUnit and bUnit tests, including LearningOutcomes/ for the course's claims
BlazorHydration.slnx        The solution
```
