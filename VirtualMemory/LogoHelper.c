// Written by Dr. William J. Blanke, November 2008
// Licensed under The Code Project Open License (CPOL)

#include "LogoHelper.h"

// SIP management

void 
LH_SIPCreate(HWND hwnd,SHACTIVATEINFO *psai)
{
#if WIN32_PLATFORM_PSPC 
	SIPINFO si; 
	int cx,cy;

	// Initialize the shell activate info structure.
	memset(psai, 0x00, sizeof (SHACTIVATEINFO));
	psai->cbSize = sizeof (SHACTIVATEINFO);

	memset(&si, 0, sizeof(si)); 
	si.cbSize = sizeof(si); 
	SHSipInfo(SPI_GETSIPINFO, 0, (PVOID) &si, FALSE); 

	cx = si.rcVisibleDesktop.right - si.rcVisibleDesktop.left; 
	cy = si.rcVisibleDesktop.bottom - si.rcVisibleDesktop.top; 

	// If the SIP is not shown, or it is showing but not docked, the 
	// desktop rect does not include the height of the menu bar. 
	if (!(si.fdwFlags & SIPF_ON) ||
		((si.fdwFlags & SIPF_ON) && !(si.fdwFlags & SIPF_DOCKED))) 
	{ 
		RECT rcMenu;
		HWND hwndMenuBar;

		hwndMenuBar = SHFindMenuBar(hwnd);
		if(hwndMenuBar!=NULL)
		{
			GetWindowRect(hwndMenuBar,&rcMenu);
			cy -= (rcMenu.bottom-rcMenu.top);
		}
	} 

	SetWindowPos(hwnd, NULL, 0, 0, cx, cy, SWP_NOMOVE | SWP_NOZORDER); 
#endif // WIN32_PLATFORM_PSPC 
}

void
LH_SIPActivate(HWND hwnd,WPARAM wParam,LPARAM lParam,SHACTIVATEINFO *psai)
{
#if WIN32_PLATFORM_PSPC 
	SHHandleWMActivate(hwnd, wParam, lParam, psai, FALSE);
#endif // WIN32_PLATFORM_PSPC
}

void
LH_SIPSettingChange(HWND hwnd,WPARAM wParam,LPARAM lParam,SHACTIVATEINFO *psai)
{
#if WIN32_PLATFORM_PSPC 
	SHHandleWMSettingChange(hwnd, wParam, lParam, psai);
#endif // WIN32_PLATFORM_PSPC
}

// Back Key management 

void
LH_BackKeyBehavior(HWND hwnd,BOOL bHasEditControl)
{
#if WIN32_PLATFORM_WFSP 
	LPARAM lparam;
	HWND hwndMenuBar;

	hwndMenuBar = SHFindMenuBar(hwnd);
	if(hwndMenuBar!=NULL)
	{
		if(bHasEditControl)
			lparam = MAKELPARAM(SHMBOF_NODEFAULT | SHMBOF_NOTIFY, SHMBOF_NODEFAULT | SHMBOF_NOTIFY);
		else
			lparam = MAKELPARAM(SHMBOF_NODEFAULT | SHMBOF_NOTIFY, 0);

		SendMessage(hwndMenuBar, SHCMBM_OVERRIDEKEY, VK_TBACK, lparam);
	}
#endif // WIN32_PLATFORM_WFSP
}

void 
LH_BackKeyHotKey(HWND hwnd,UINT uMessage,WPARAM wParam,LPARAM lParam)
{
#if WIN32_PLATFORM_WFSP 
	if(HIWORD(lParam) == VK_TBACK)
		SHSendBackToFocusWindow(uMessage, wParam, lParam);
#endif // WIN32_PLATFORM_WFSP
}

// Spinner control management

typedef struct
{
	WNDPROC origCombo;
	HWND hwndSpin;
	HWND hwndUpDown;
} SPINNERCOMBO,*PSPINNERCOMBO;

LRESULT CALLBACK SubclassComboProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	PSPINNERCOMBO psc;
	WNDPROC origCombo;

	psc = (PSPINNERCOMBO)GetProp(hWnd, TEXT("SpinComboData"));
	origCombo=psc->origCombo;

	switch (message)
	{
		case CB_ADDSTRING:
			return SendMessage(psc->hwndSpin,LB_ADDSTRING,wParam,lParam);
		case CB_SETITEMDATA:
			return SendMessage(psc->hwndSpin,LB_SETITEMDATA,wParam,lParam);
		case CB_GETITEMDATA:
			return SendMessage(psc->hwndSpin,LB_GETITEMDATA,wParam,lParam);
		case CB_SETCURSEL:
			return SendMessage(psc->hwndSpin,LB_SETCURSEL,wParam,lParam);
		case CB_GETCURSEL:
			return SendMessage(psc->hwndSpin,LB_GETCURSEL,wParam,lParam);
		case WM_SIZE:
		{
			RECT rc;

			GetWindowRect(hWnd,&rc);
			MapWindowPoints (NULL, GetParent(hWnd), (LPPOINT)&rc, 2);
			MoveWindow(psc->hwndSpin,rc.left,rc.top,
				rc.right-rc.left,rc.bottom-rc.top,TRUE);
			SendMessage (psc->hwndUpDown, UDM_SETBUDDY, (WPARAM)psc->hwndSpin, 0);
			break;
		}
		case WM_DESTROY:
		{
			SetWindowLong (hWnd, GWL_WNDPROC, (LONG_PTR)psc->origCombo);
			free(psc);
			break;
		}
	}
	return CallWindowProc (origCombo, hWnd, message, wParam, lParam);
}

void LH_InitSpinCombo(HWND hWnd)
{
#if WIN32_PLATFORM_WFSP 
	PSPINNERCOMBO psc;

	psc=(PSPINNERCOMBO)malloc(sizeof(SPINNERCOMBO));

	if(psc!=NULL)
	{
		HFONT hFont;
		RECT rc;

		memset(psc,0x00,sizeof(SPINNERCOMBO));

		SetProp(hWnd, TEXT("SpinComboData"), psc);
		psc->origCombo = (WNDPROC)SetWindowLong (hWnd, GWL_WNDPROC, (LONG_PTR)SubclassComboProc);

		GetWindowRect(hWnd,&rc);
		MapWindowPoints (NULL, GetParent(hWnd), (LPPOINT)&rc, 2);

		psc->hwndSpin = CreateWindow (TEXT("listbox"), NULL, 
			WS_VISIBLE | WS_TABSTOP | LBS_NOTIFY, 
			rc.left, rc.top, rc.right-rc.left, rc.bottom-rc.top, 
			GetParent(hWnd), (HMENU)GetDlgCtrlID(hWnd), NULL, NULL);

		// Put spinner in the proper tab order after the original combobox
		SetWindowPos(psc->hwndSpin,hWnd,0,0,0,0,SWP_NOSIZE|SWP_NOMOVE|SWP_NOACTIVATE);

		psc->hwndUpDown = CreateWindow (UPDOWN_CLASS, NULL, 
			WS_VISIBLE | UDS_HORZ | UDS_ALIGNRIGHT | UDS_ARROWKEYS | 
			UDS_SETBUDDYINT | UDS_WRAP | UDS_EXPANDABLE,
			0, 0, 0, 0, GetParent(hWnd), NULL, NULL, NULL);

		SendMessage (psc->hwndUpDown, UDM_SETBUDDY, (WPARAM)psc->hwndSpin, 0);

		hFont = (HFONT)SendMessage(hWnd, WM_GETFONT, 0, 0);
		SendMessage(psc->hwndSpin,WM_SETFONT,(WPARAM)hFont,0);

		EnableWindow(hWnd,FALSE);
		ShowWindow(hWnd,SW_HIDE);
	}
#endif // WIN32_PLATFORM_WFSP
}
