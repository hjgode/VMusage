#ifndef Included_LogoHelper_h	/* [ */
#define Included_LogoHelper_h

#include <windows.h>
#include <aygshell.h>
#include <tpcshell.h>
#include <CommCtrl.h>

#ifdef __cplusplus
extern "C" {
#endif

// SIP management
void 
LH_SIPCreate(HWND hwnd,SHACTIVATEINFO *psai);

void
LH_SIPActivate(HWND hwnd,WPARAM wParam,LPARAM lParam,SHACTIVATEINFO *psai);

void
LH_SIPSettingChange(HWND hwnd,WPARAM wParam,LPARAM lParam,SHACTIVATEINFO *psai);

// Back Key management 
void
LH_BackKeyBehavior(HWND hwnd,BOOL bHasEditControl);

void 
LH_BackKeyHotKey(HWND hwnd,UINT uMessage,WPARAM wParam,LPARAM lParam);

// Spinner control management
void 
LH_InitSpinCombo(HWND hWnd);

#ifdef __cplusplus
}
#endif

#endif /* ] Included_LogoHelper_h */