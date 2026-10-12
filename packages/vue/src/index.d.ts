import type { App, InjectionKey } from 'vue'
import type { AxiosInstance } from 'axios'
import type { PiniaPlugin } from 'pinia'

export interface InteropCapabilityDescriptor {
  id: string
  version: string
  operations: string[]
  description?: string | null
}

export interface QuasarInteropClient {
  capabilities(): Promise<InteropCapabilityDescriptor[]>
  invoke<T = unknown>(capabilityId: string, version: string, operation: string, args?: unknown): Promise<T>
}

export declare const QUASAR_INTEROP_KEY: InjectionKey<QuasarInteropClient>
export declare function createQuasarInterop(axios: AxiosInstance, options?: { baseURL?: string }): QuasarInteropClient
export declare function installQuasarInterop(app: App, client: QuasarInteropClient): void
export declare function useQuasarInterop(): QuasarInteropClient
export declare function createQuasarInteropPiniaPlugin(client: QuasarInteropClient): PiniaPlugin
