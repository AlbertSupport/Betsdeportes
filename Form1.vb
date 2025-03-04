Imports System.Runtime.InteropServices

Public Class Form1

    Private Const WH_MOUSE_LL As Integer = 14
    Private Const WM_LBUTTONDOWN As Integer = &H201
    Private currentUrl As String = "https://shop.passowin.net/#/"

    Private Delegate Function LowLevelMouseProc(nCode As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    Private mouseProc As LowLevelMouseProc = AddressOf MouseHookCallback
    Private hookId As IntPtr = IntPtr.Zero


    <StructLayout(LayoutKind.Sequential)>
    Private Structure MSLLHOOKSTRUCT
        Public pt As Point
        Public mouseData As UInteger
        Public flags As UInteger
        Public time As UInteger
        Public dwExtraInfo As IntPtr
    End Structure


    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowsHookEx(idHook As Integer, lpfn As LowLevelMouseProc, hMod As IntPtr, dwThreadId As UInteger) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function UnhookWindowsHookEx(hhk As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function CallNextHookEx(hhk As IntPtr, nCode As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function GetModuleHandle(lpModuleName As String) As IntPtr
    End Function


    Private Function MouseHookCallback(nCode As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
        If nCode >= 0 AndAlso wParam = CType(WM_LBUTTONDOWN, IntPtr) Then
            Dim mouseInfo As MSLLHOOKSTRUCT = Marshal.PtrToStructure(Of MSLLHOOKSTRUCT)(lParam)
            Dim cursorPosition As Point = mouseInfo.pt


            If Not Me.Bounds.Contains(cursorPosition) Then
                Me.Close()
            End If
        End If

        Return CallNextHookEx(hookId, nCode, wParam, lParam)
    End Function

    Private Async Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Icon = New Icon("Icon/icono.ico")
        Me.ShowInTaskbar = False

        Me.WindowState = FormWindowState.Maximized

        Me.TopMost = False

        Try
            Await WebView21.EnsureCoreWebView2Async()

            WebView21.CoreWebView2.Navigate(currentUrl)

            AddHandler WebView21.KeyDown, AddressOf WebView21_KeyDown
            AddHandler WebView21.CoreWebView2.NavigationStarting, AddressOf WebView21_NavigationStarting
            AddHandler WebView21.CoreWebView2.NavigationCompleted, AddressOf RemoveBackgroundScript
            WebView21.CoreWebView2.Settings.IsStatusBarEnabled = False
            WebView21.CoreWebView2.Settings.AreDevToolsEnabled = False

        Catch ex As Exception
            MessageBox.Show("Error inicializando WebView2: " & ex.Message)
        End Try

    End Sub

    Private Sub WebView21_KeyDown(sender As Object, e As KeyEventArgs)

        If e.KeyCode = Keys.F12 Then
            e.SuppressKeyPress = True
            e.Handled = True
            Return
        End If

        If e.KeyCode = Keys.F8 Then
            e.SuppressKeyPress = True
            e.Handled = True
        End If

        If e.KeyCode = Keys.F3 Then

            currentUrl = "https://shop.passowin.net/#/config"
            WebView21.CoreWebView2.Navigate(currentUrl)

        ElseIf e.KeyCode = Keys.F5 Then

            currentUrl = "https://shop.passowin.net/#/"
            WebView21.CoreWebView2.Navigate(currentUrl)


            WebView21.CoreWebView2.Reload()
        End If
    End Sub

    Private Sub WebView21_NavigationStarting(sender As Object, e As Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs)
        If e.Uri <> currentUrl Then
            e.Cancel = True
        End If
    End Sub



    Private Sub RemoveBackgroundScript(sender As Object, e As Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs)
        Dim script As String = "
    (function() {
        function applyChanges() {
            

            let logoElement = document.querySelector('.logo');
            if (logoElement) {
                logoElement.style.setProperty('background', 'none', 'important');
            }
            
            let logoElement2 = document.querySelector('.cpnContent');
            if (logoElement2) {
                logoElement2.style.setProperty('background', 'none', 'important');
                logoElement2.style.backgroundColor = '#B6B6BB'; 
            }
                
          
                
            let logoutButton = document.querySelector('li.logOut');
            if (logoutButton) {
                logoutButton.parentNode.removeChild(logoutButton);
            }

            let checkIcons = document.querySelectorAll('li.checkcppn');
            if (checkIcons.length > 3 && checkIcons[3].parentNode) {
                checkIcons[3].parentNode.removeChild(checkIcons[3]);
            }
            


            let paises = document.querySelector('li#langOpen');
            if (paises && paises.parentNode) {
                paises.parentNode.removeChild(paises);
            }

            let keyboardShortcut = document.querySelector('span.keyboardshortcut4');
            if (keyboardShortcut && keyboardShortcut.parentNode) {
                keyboardShortcut.parentNode.removeChild(keyboardShortcut);
            }

            let keyboardShortcut2 = document.querySelector('span.keyboardshortcut2');
            if (keyboardShortcut2 && keyboardShortcut2.parentNode) {
                keyboardShortcut2.parentNode.removeChild(keyboardShortcut2);
            }

            let keyboardShortcut3 = document.querySelector('span.keyboardshortcut');
            if (keyboardShortcut3 && keyboardShortcut3.parentNode) {
                keyboardShortcut3.parentNode.removeChild(keyboardShortcut3);
            }
        }

        // Aplicar cambios inmediatamente
        applyChanges();

        // Observar cambios en el DOM y aplicar cambios dinámicamente
        let observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                applyChanges();
            });
        });

        observer.observe(document.body, { childList: true, subtree: true });
    })();
"

        WebView21.CoreWebView2.ExecuteScriptAsync(script)


    End Sub


    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If hookId <> IntPtr.Zero Then
            UnhookWindowsHookEx(hookId)
        End If
    End Sub


End Class


