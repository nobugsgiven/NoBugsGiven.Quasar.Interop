import { inject } from 'vue'

export const QUASAR_INTEROP_KEY = Symbol.for('nobugsgiven.quasar-interop')

/** Create a typed-capability HTTP client using the consuming app's Axios instance. */
export function createQuasarInterop(axios, { baseURL = '/api/interop/v1' } = {}) {
  if (!axios || typeof axios.get !== 'function' || typeof axios.post !== 'function') {
    throw new TypeError('createQuasarInterop requires an Axios-compatible client.')
  }

  return Object.freeze({
    async capabilities() {
      const { data } = await axios.get(`${baseURL}/capabilities`)
      return data?.capabilities ?? []
    },

    async invoke(capabilityId, version, operation, argumentsValue = {}) {
      if (!capabilityId || !version || !operation) {
        throw new TypeError('A capability id, version, and operation are required.')
      }
      const { data } = await axios.post(`${baseURL}/invoke`, {
        capabilityId,
        version,
        operation,
        arguments: argumentsValue,
      })
      return data?.result
    },
  })
}

/** Install the bridge for components to obtain with useQuasarInterop(). */
export function installQuasarInterop(app, client) {
  app.provide(QUASAR_INTEROP_KEY, client)
}

export function useQuasarInterop() {
  const client = inject(QUASAR_INTEROP_KEY, null)
  if (!client) throw new Error('Quasar interop has not been installed in this Vue app.')
  return client
}

/** Optional Pinia plugin: stores receive the same host bridge as Vue components. */
export function createQuasarInteropPiniaPlugin(client) {
  return () => ({ $quasarInterop: client })
}
