# NoBugsGiven.Quasar.Interop

Shared contracts and a Vue client for Quasar applications that communicate with a .NET
host through explicitly granted, versioned capabilities. The UI remains a normal
Quasar/Vue application; this library does not replace Razor components or require a
.NET WebAssembly UI.

## .NET host

Call `AddQuasarInterop()` during service registration and `MapQuasarInterop()` when
mapping endpoints. The default authorization policy denies every capability. Hosts
must register their own explicit capability handlers and authorization policy.

## Vue client

Install `@nobugsgiven/quasar-interop` in the Quasar/Vite project and pass the app's
Axios instance:

```js
import { createQuasarInterop, installQuasarInterop } from '@nobugsgiven/quasar-interop'
import { api } from './api'

installQuasarInterop(app, createQuasarInterop(api))
```

Components can use `useQuasarInterop()` and Pinia stores can use the optional
`createQuasarInteropPiniaPlugin()` adapter. The client invokes only capabilities
advertised by the host; authorization is enforced server-side on every operation.

The package is authored by **No Bugs Given** (`nobugsgiven@gmail.com`). Its repository
will be hosted under `nobugsgiven` and referenced from the MediaPager superproject as a
cross-owner submodule. This local checkout has no remote configured.
