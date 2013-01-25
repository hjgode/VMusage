// Written by Dr. William J. Blanke, November 2008
// Licensed under The Code Project Open License (CPOL)

#include "LogoHelper.h"
#include <tlhelp32.h>
#include "resource.h"

#ifndef TH32CS_SNAPNOHEAPS
#define TH32CS_SNAPNOHEAPS 0x40000000 
#endif

#define APPNAME TEXT("Virtual Memory")

#define NUMPAGES 8192
#define STARTBAR 1
#define NUMBARS 32

#define VMEMCOMMIT (MEM_COMMIT>>12)
#define VMEMRESERVE (MEM_RESERVE>>12)

HINSTANCE g_hinst;

typedef struct 
{
	SHACTIVATEINFO sai;
	int screenWidth;
	int screenHeight;
	int chartWidth;
	int chartHeight;
	HBITMAP hBmp;
	int barWidth;
	int focusSlot;
	WCHAR szExeName[NUMBARS][MAX_PATH];
	BOOL pageAllocated[NUMBARS][NUMPAGES];
} VIRTUALDATA;

void GetVirtualMemoryStatus(VIRTUALDATA *pvd)
{
	MEMORY_BASIC_INFORMATION mbi;
	int idx;
	DWORD addr;
	BYTE state;

	memset(pvd->pageAllocated,0x00,sizeof(pvd->pageAllocated));

	for(idx=STARTBAR;idx<STARTBAR+NUMBARS;idx++)
	{
		DWORD offset;

		addr = idx * 0x02000000;

		for( offset = 0; offset < 0x02000000; offset += mbi.RegionSize )
		{
			unsigned int i;

			memset(&mbi,0x00,sizeof(MEMORY_BASIC_INFORMATION));

			if(VirtualQuery( (void*)( addr + offset ), &mbi, sizeof( MEMORY_BASIC_INFORMATION ) )==0)
				break;

			state=(BYTE)((mbi.State>>12)&(VMEMCOMMIT|VMEMRESERVE));

			if(state)
			{
				for(i=(offset)/4096;i<(offset+mbi.RegionSize)/4096;i++)
					pvd->pageAllocated[idx-STARTBAR][i]=state;
			}
		}
	}
}

void GetProcessNames(VIRTUALDATA *pvd)
{
	HANDLE hProcessSnap;
	HANDLE hProcess;
	PROCESSENTRY32 pe32;
	int slot;

	for(slot=STARTBAR;slot<STARTBAR+NUMBARS;slot++)
		swprintf(pvd->szExeName[slot-STARTBAR],TEXT("Slot %d: empty"),slot);

	if((1-STARTBAR)>=0)
		wcscpy(pvd->szExeName[1-STARTBAR],TEXT("Slot 1: ROM DLLs"));

	// Take a snapshot of all processes in the system.
	hProcessSnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS|TH32CS_SNAPNOHEAPS, 0 );
	if( hProcessSnap != INVALID_HANDLE_VALUE )
	{
		memset(&pe32,0x00,sizeof(PROCESSENTRY32));
		pe32.dwSize = sizeof( PROCESSENTRY32 );

		if( Process32First( hProcessSnap, &pe32 ) )
		{
			do
			{
				hProcess = OpenProcess( PROCESS_QUERY_INFORMATION, FALSE, pe32.th32ProcessID );
				if( hProcess != NULL )
				{
					slot=pe32.th32MemoryBase/0x02000000;

					if(slot-STARTBAR<NUMBARS)
						swprintf(pvd->szExeName[slot-STARTBAR],TEXT("Slot %d: %s"),slot,pe32.szExeFile);

					CloseHandle( hProcess );
				}
			} while( Process32Next( hProcessSnap, &pe32 ) );
		}

		CloseToolhelp32Snapshot( hProcessSnap );
	}
}

void CreateVirtualMemoryGraph(VIRTUALDATA *pvd)
{
	int bar,tic,pageStart,pagesPerTic,w;
	int empty,reserve,commit;
	COLORREF color;
	int page;
	HDC hdc,hdcMem;
	HBITMAP hOldBmp;

	if(pvd->hBmp!=NULL)
	{
		DeleteObject(pvd->hBmp);
		pvd->hBmp=NULL;
	}

	pvd->barWidth=pvd->screenWidth/NUMBARS; // round down
	pagesPerTic=(NUMPAGES+pvd->screenHeight-1)/pvd->screenHeight; // round up

	pvd->chartWidth=pvd->barWidth*NUMBARS-1;
	pvd->chartHeight=NUMPAGES/pagesPerTic;

	hdc=GetDC(NULL);

	if(hdc!=NULL)
	{
		hdcMem = CreateCompatibleDC (hdc);

		if(hdcMem!=NULL)
		{
			pvd->hBmp = CreateCompatibleBitmap (hdc, pvd->chartWidth, pvd->chartHeight);

			if(pvd->hBmp!=NULL)
			{
				RECT rc;

				hOldBmp=(HBITMAP)SelectObject(hdcMem,pvd->hBmp);

				rc.left=rc.top=0;
				rc.right=pvd->chartWidth;
				rc.bottom=pvd->chartHeight;

				FillRect(hdcMem, &rc, (HBRUSH)GetStockObject(WHITE_BRUSH));

				for(tic=0;tic<pvd->chartHeight;tic++)
				{
					for(bar=STARTBAR;bar<STARTBAR+NUMBARS;bar++)
					{
						pageStart=tic*pagesPerTic;

						empty=reserve=commit=0;

						for(page=pageStart;page<pageStart+pagesPerTic;page++)
						{
							if(pvd->pageAllocated[bar-STARTBAR][page]&VMEMCOMMIT)
								commit++;
							else if(pvd->pageAllocated[bar-STARTBAR][page]&VMEMRESERVE)
								reserve++;
							else
								empty++;
						}

						color=RGB(empty*255/pagesPerTic,
							commit*255/pagesPerTic,
							reserve*255/pagesPerTic);

						for(w=0;w<pvd->barWidth-1;w++)
						{
							SetPixel(hdcMem,((bar-STARTBAR)*pvd->barWidth)+w,pvd->chartHeight-tic,color);
						}
					}
				}
				SelectObject(hdcMem,hOldBmp);
			}
			DeleteDC(hdcMem);
		}
		ReleaseDC(NULL,hdc);
	}
}

BOOL CALLBACK DlgProc (HWND hWnd, UINT wMsg, WPARAM wParam, LPARAM lParam) 
{
	VIRTUALDATA *pvd;
	pvd=(VIRTUALDATA *)GetWindowLong (hWnd, GWL_USERDATA);

	switch (wMsg) 
	{
		case WM_INITDIALOG:
		{
			SHMENUBARINFO mbi;
			SHINITDLGINFO shidi;

			SetWindowLong(hWnd,GWL_USERDATA,lParam);

			pvd=(VIRTUALDATA *)lParam;

			memset(&shidi,0x00,sizeof(SHINITDLGINFO));
			shidi.dwMask = SHIDIM_FLAGS;
			shidi.dwFlags = SHIDIF_SIZEDLGFULLSCREEN;
			shidi.hDlg = hWnd;
			SHInitDialog (&shidi);

			memset(&mbi, 0x00, sizeof(SHMENUBARINFO));
			mbi.cbSize     = sizeof(SHMENUBARINFO);
			mbi.hwndParent = hWnd;
			mbi.nToolBarId = IDR_MENU1;
			mbi.dwFlags	   = SHCMBF_HMENU;
			mbi.hInstRes   = g_hinst;
			SHCreateMenuBar(&mbi); 

			SetWindowText(hWnd,APPNAME);

			LH_SIPCreate(hWnd,&(pvd->sai));

			SetDlgItemText(hWnd,IDC_SLOTTEXT,pvd->szExeName[pvd->focusSlot]);
			return TRUE;
		}
		case WM_ACTIVATE:
			LH_SIPActivate(hWnd, wParam, lParam, &(pvd->sai));
			break;
		case WM_SETTINGCHANGE:
			LH_SIPSettingChange(hWnd, wParam, lParam, &(pvd->sai));
			break;
		case WM_KEYUP:
		{
			switch (wParam) 
			{
				case VK_LEFT :
				case VK_RIGHT :
					pvd->focusSlot=pvd->focusSlot+NUMBARS;
					if(wParam==VK_LEFT)
						pvd->focusSlot--;
					else
						pvd->focusSlot++;
					pvd->focusSlot=pvd->focusSlot%NUMBARS;
					SetDlgItemText(hWnd,IDC_SLOTTEXT,pvd->szExeName[pvd->focusSlot]);
					InvalidateRect(hWnd,NULL,FALSE); // Don't flash
					break;
			}
			break;
		}
		case WM_SIZE:
		{
			RECT rc,rcItem;
			int sizeWidth,sizeHeight;

			GetClientRect(hWnd,&rc);

			GetWindowRect(GetDlgItem(hWnd,IDC_SLOTTEXT),&rcItem);
			MapWindowPoints (NULL, hWnd, (LPPOINT)&rcItem, 2);
			MoveWindow(GetDlgItem(hWnd,IDC_SLOTTEXT),rcItem.left,rcItem.top,
				rc.right-rcItem.left,rcItem.bottom-rcItem.top,TRUE);

			sizeWidth=rc.right-rc.left;
			sizeHeight=rc.bottom-rcItem.bottom;

			if((sizeWidth!=pvd->screenWidth)||(sizeHeight!=pvd->screenHeight))
			{
				HCURSOR oldCursor;

				oldCursor=SetCursor(LoadCursor(NULL, IDC_WAIT)); 
				pvd->screenWidth=sizeWidth;
				pvd->screenHeight=sizeHeight;
				CreateVirtualMemoryGraph(pvd);
				SetCursor(oldCursor);

				InvalidateRect(hWnd,NULL,TRUE);
			}
			break;
		}
        case WM_PAINT:
		{
			RECT rc,rcItem,rcFocus;
			HDC hdc,hdcMem;
			HBITMAP hOldBmp;
			PAINTSTRUCT ps;
			int origx,origy;

			if(pvd->hBmp==NULL)
				break;

			GetClientRect(hWnd,&rc);

			GetWindowRect(GetDlgItem(hWnd,IDC_SLOTTEXT),&rcItem);
			MapWindowPoints (NULL, hWnd, (LPPOINT)&rcItem, 2);

			origx=rc.left+(rc.right-rc.left-pvd->chartWidth)/2;
			origy=rcItem.bottom+(rc.bottom-rcItem.bottom-pvd->chartHeight)/2;

			hdc = BeginPaint(hWnd, &ps);

			hdcMem = CreateCompatibleDC (hdc);
			if(hdcMem!=NULL)
			{
				hOldBmp=(HBITMAP)SelectObject(hdcMem,pvd->hBmp);
				BitBlt(hdc,origx,origy,pvd->chartWidth,pvd->chartHeight,hdcMem,0,0,SRCCOPY);
				SelectObject(hdcMem,hOldBmp);
				DeleteDC(hdcMem);
			}

			rcFocus.left=origx+(pvd->barWidth*pvd->focusSlot);
			rcFocus.right=rcFocus.left+pvd->barWidth-1;
			rcFocus.top=origy;
			rcFocus.bottom=rcFocus.top+pvd->chartHeight;
			DrawFocusRect (hdc, &rcFocus);

            EndPaint(hWnd, &ps);
            break;
		}
		case WM_CLOSE:
		{
			EndDialog(hWnd, TRUE);
			break;
		}
		case WM_COMMAND:
		{
			switch (LOWORD (wParam)) 
			{		
				case IDC_SNAPSHOT:
				{
					HCURSOR oldCursor;
					SetDlgItemText(hWnd,IDC_SLOTTEXT,TEXT("Taking snapshot..."));
					UpdateWindow(hWnd);
					oldCursor=SetCursor(LoadCursor(NULL, IDC_WAIT)); 
					GetVirtualMemoryStatus(pvd);
					GetProcessNames(pvd);
					CreateVirtualMemoryGraph(pvd);
					SetDlgItemText(hWnd,IDC_SLOTTEXT,pvd->szExeName[pvd->focusSlot]);
					SetCursor(oldCursor); 
					InvalidateRect(hWnd,NULL,TRUE);
					break;
				}
				case IDC_EXIT: // From our menubar
					EndDialog(hWnd, TRUE);
					break;
			}
			break;
		}
	}
	return FALSE;
}

int WINAPI WinMain (HINSTANCE hInstance, HINSTANCE hPrevInstance,
                    LPWSTR lpCmdLine, int nCmdShow) 
{
	VIRTUALDATA *pvd;
    HANDLE hSem;

    hSem = CreateSemaphore (NULL, 0, 1, APPNAME);
    if ((hSem != NULL) && (GetLastError() == ERROR_ALREADY_EXISTS)) 
	{
		HWND hWndExisting;

        CloseHandle(hSem);

		hWndExisting = FindWindow (NULL, APPNAME);
		if (hWndExisting) 
			SetForegroundWindow ((HWND)(((ULONG)hWndExisting) | 0x01));

		return TRUE;
	}

	g_hinst=hInstance;
	InitCommonControls();

	pvd=(VIRTUALDATA *)malloc(sizeof(VIRTUALDATA));

	if(pvd!=NULL)
	{
		memset(pvd,0x00,sizeof(VIRTUALDATA));

		GetVirtualMemoryStatus(pvd);
		GetProcessNames(pvd);

		DialogBoxParam (g_hinst, MAKEINTRESOURCE(IDD_DIALOG1), NULL, DlgProc, (LPARAM)pvd);

		free(pvd);
	}
	return 0;
}
