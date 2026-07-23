// EclipseCore.dll - Injected into Minecraft (javaw.exe)
// Build with: cl /LD /EHsc dllmain.cpp /Fe:EclipseCore.dll

#include <windows.h>
#include <stdio.h>

static HANDLE g_mapFile = NULL;
static LPVOID g_mapView = NULL;
static volatile BOOL g_running = TRUE;

DWORD WINAPI BridgeThread(LPVOID param) {
    (void)param;
    while (g_running) {
        if (g_mapView) {
            // Read module state from shared memory (written by Eclipse Client)
            // Real implementation hooks Minecraft JVM here
        }
        Sleep(50);
    }
    return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID reserved) {
    (void)reserved;
    switch (reason) {
        case DLL_PROCESS_ATTACH:
            DisableThreadLibraryCalls(hModule);
            g_mapFile = OpenFileMappingA(FILE_MAP_ALL_ACCESS, FALSE, "EclipseClient_IPC_v1");
            if (g_mapFile) {
                g_mapView = MapViewOfFile(g_mapFile, FILE_MAP_ALL_ACCESS, 0, 0, 65536);
            }
            CreateThread(NULL, 0, BridgeThread, NULL, 0, NULL);
            break;
        case DLL_PROCESS_DETACH:
            g_running = FALSE;
            if (g_mapView) UnmapViewOfFile(g_mapView);
            if (g_mapFile) CloseHandle(g_mapFile);
            break;
    }
    return TRUE;
}
