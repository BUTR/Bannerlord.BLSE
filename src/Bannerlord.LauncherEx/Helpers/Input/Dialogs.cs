﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using Vanara.InteropServices;

namespace Bannerlord.LauncherEx.Helpers;

/// <summary>
/// An error code returned by the CommDlgExtendedError function.
/// </summary>
/// <remarks>
/// <list type="table">
/// <listheader>
/// <term>Error code</term>
/// <term>Meaning</term>
/// </listheader>
/// <item>
/// <term>CDERR</term>
/// <term>General error codes that can be returned for any of the common dialog box functions.</term>
/// </item>
/// <item>
/// <term>PDERR</term>
/// <term>Error codes returned for the PrintDlg function.</term>
/// </item>
/// <item>
/// </item>
/// <item>
/// <term>CFERR</term>
/// <term>Error codes returned for the ChooseFont function.</term>
/// </item>
/// <item>
/// <term>FNERR</term>
/// <term>Error codes returned for the GetOpenFileName and GetSaveFileName functions.</term>
/// </item>
/// <item>
/// <term>FRERR</term>
/// <term>Error codes returned for the FindText and ReplaceText functions.</term>
/// </item>
/// </list>
/// </remarks>
file enum ERR : uint
{
    /// <summary>
    /// The dialog box could not be created. The common dialog box function's call to the DialogBox function failed. For example,
    /// this error occurs if the common dialog box call specifies an invalid window handle.
    /// </summary>
    CDERR_DIALOGFAILURE = 0xFFFF,

    /// <summary>
    /// The common dialog box function failed to find a specified resource.
    /// </summary>
    CDERR_FINDRESFAILURE = 0x0006,

    /// <summary>
    ///  The common dialog box function failed during initialization. This error often occurs when sufficient memory is not available.
    /// </summary>
    CDERR_INITIALIZATION = 0x0002,

    /// <summary>
    /// The common dialog box function failed to load a specified resource.
    /// </summary>
    CDERR_LOADRESFAILURE = 0x0007,

    /// <summary>
    /// The common dialog box function failed to load a specified string.
    /// </summary>
    CDERR_LOADSTRFAILURE = 0x0005,

    /// <summary>
    /// The common dialog box function failed to lock a specified resource.
    /// </summary>
    CDERR_LOCKRESFAILURE = 0x0008,

    /// <summary>
    /// The common dialog box function was unable to allocate memory for internal structures.
    /// </summary>
    CDERR_MEMALLOCFAILURE = 0x0009,

    /// <summary>
    /// The common dialog box function was unable to lock the memory associated with a handle.
    /// </summary>
    CDERR_MEMLOCKFAILURE = 0x000A,

    /// <summary>
    /// The <c>ENABLETEMPLATE</c> flag was set in the <c>Flags</c> member of the initialization structure for the corresponding common
    /// dialog box, but you failed to provide a corresponding instance handle.
    /// </summary>
    CDERR_NOHINSTANCE = 0x0004,

    /// <summary>
    /// The <c>ENABLEHOOK</c> flag was set in the <c>Flags</c> member of the initialization structure for the corresponding common
    /// dialog box, but you failed to provide a pointer to a corresponding hook procedure.
    /// </summary>
    CDERR_NOHOOK = 0x000B,

    /// <summary>
    /// The <c>ENABLETEMPLATE</c> flag was set in the <c>Flags</c> member of the initialization structure for the corresponding common dialog
    /// box, but you failed to provide a corresponding template.
    /// </summary>
    CDERR_NOTEMPLATE = 0x0003,

    /// <summary>
    /// The RegisterWindowMessage function returned an error code when it was called by the common dialog box function.
    /// </summary>
    CDERR_REGISTERMSGFAIL = 0x000C,

    /// <summary>
    /// The <c>lStructSize</c> member of the initialization structure for the corresponding common dialog box is invalid.
    /// </summary>
    CDERR_STRUCTSIZE = 0x0001,

    /// <summary>
    /// The PrintDlg function failed when it attempted to create an information context.
    /// </summary>
    PDERR_CREATEICFAILURE = 0x100A,

    /// <summary>
    /// You called the PrintDlg function with the <c>DN_DEFAULTPRN</c> flag specified in the <c>wDefault</c> member of the <c>DEVNAMES</c> structure,
    /// but the printer described by the other structure members did not match the current default printer. This error occurs when
    /// you store the <c>DEVNAMES</c> structure, and the user changes the default printer by using the Control Panel.
    /// <para>To use the printer described by the <c>DEVNAMES</c> structure, clear the <c>DN_DEFAULTPRN</c> flag and call PrintDlg again.</para>
    /// <para>To use the default printer, replace the <c>DEVNAMES</c> structure (and the structure, if one exists) with <c>NULL</c>; and call PrintDlg again.</para>
    /// </summary>
    PDERR_DEFAULTDIFFERENT = 0x100C,

    /// <summary>
    /// The data in the <c>DEVMODE</c> and <c>DEVNAMES</c> structures describes two different printers.
    /// </summary>
    PDERR_DNDMMISMATCH = 0x1009,

    /// <summary>
    /// The printer driver failed to initialize a <c>DEVMODE</c> structure.
    /// </summary>
    PDERR_GETDEVMODEFAIL = 0x1005,

    /// <summary>
    /// The PrintDlg function failed during initialization, and there is no more specific extended error code to describe the failure.
    /// This is the generic default error code for the function.
    /// </summary>
    PDERR_INITFAILURE = 0x1006,

    /// <summary>
    /// The PrintDlg function failed to load the device driver for the specified printer.
    /// </summary>
    PDERR_LOADDRVFAILURE = 0x1004,

    /// <summary>
    /// A default printer does not exist.
    /// </summary>
    PDERR_NODEFAULTPRN = 0x1008,

    /// <summary>
    /// No printer drivers were found.
    /// </summary>
    PDERR_NODEVICES = 0x1007,

    /// <summary>
    /// The PrintDlg function failed to parse the strings in the [devices] section of the WIN.INI file.
    /// </summary>
    PDERR_PARSEFAILURE = 0x1002,

    /// <summary>
    /// The [devices] section of the WIN.INI file did not contain an entry for the requested printer.
    /// </summary>
    PDERR_PRINTERNOTFOUND = 0x100B,

    /// <summary>
    /// The <c>PD_RETURNDEFAULT</c> flag was specified in the Flags member of the <c>PRINTDLG</c> structure, but the <c>hDevMode</c> or <c>hDevNames</c> member was not <c>NULL</c>.
    /// </summary>
    PDERR_RETDEFFAILURE = 0x1003,

    /// <summary>
    /// The PrintDlg function failed to load the required resources.
    /// </summary>
    PDERR_SETUPFAILURE = 0x1001,

    /// <summary>
    /// The size specified in the <c>nSizeMax</c> member of the <c>CHOOSEFONT</c> structure is less than the size specified in the <c>nSizeMin</c> member.
    /// </summary>
    CFERR_MAXLESSTHANMIN = 0x2002,

    /// <summary>
    /// No fonts exist.
    /// </summary>
    CFERR_NOFONTS = 0x2001,

    /// <summary>
    /// The buffer pointed to by the <c>lpstrFile</c> member of the <c>OPENFILENAME</c> structure is too small for the file name specified
    /// by the user. The first two bytes of the <c>lpstrFile</c> buffer contain an integer value specifying the size required to receive
    /// the full name, in characters.
    /// </summary>
    FNERR_BUFFERTOOSMALL = 0x3003,

    /// <summary>
    /// A file name is invalid.
    /// </summary>
    FNERR_INVALIDFILENAME = 0x3002,

    /// <summary>
    /// An attempt to subclass a list box failed because sufficient memory was not available.
    /// </summary>
    FNERR_SUBCLASSFAILURE = 0x3001,

    /// <summary>
    /// A member of the <c>FINDREPLACE</c> structure points to an invalid buffer.
    /// </summary>
    FRERR_BUFFERLENGTHZERO = 0x4001,
}

/// <summary>
/// A set of bit flags you can use to initialize the dialog box. When the dialog box returns, it sets these flags to indicate the
/// user's input.
/// </summary>
[Flags]
file enum OFN
{
    /// <summary>
    /// The File Name list box allows multiple selections. If you also set the OFN_EXPLORER flag, the dialog box uses the
    /// Explorer-style user interface; otherwise, it uses the old-style user interface.
    /// <para>
    /// If the user selects more than one file, the lpstrFile buffer returns the path to the current directory followed by the file
    /// names of the selected files. The nFileOffset member is the offset, in bytes or characters, to the first file name, and the
    /// nFileExtension member is not used. For Explorer-style dialog boxes, the directory and file name strings are NULL separated,
    /// with an extra NULL character after the last file name. This format enables the Explorer-style dialog boxes to return long
    /// file names that include spaces. For old-style dialog boxes, the directory and file name strings are separated by spaces and
    /// the function uses short file names for file names with spaces. You can use the FindFirstFile function to convert between
    /// long and short file names.
    /// </para>
    /// <para>
    /// If you specify a custom template for an old-style dialog box, the definition of the File Name list box must contain the
    /// LBS_EXTENDEDSEL value.
    /// </para>
    /// </summary>
    OFN_ALLOWMULTISELECT = 0x00000200,

    /// <summary>
    /// If the user specifies a file that does not exist, this flag causes the dialog box to prompt the user for permission to
    /// create the file. If the user chooses to create the file, the dialog box closes and the function returns the specified name;
    /// otherwise, the dialog box remains open. If you use this flag with the OFN_ALLOWMULTISELECT flag, the dialog box allows the
    /// user to specify only one nonexistent file.
    /// </summary>
    OFN_CREATEPROMPT = 0x00002000,

    /// <summary>
    /// Prevents the system from adding a link to the selected file in the file system directory that contains the user's most
    /// recently used documents. To retrieve the location of this directory, call the SHGetSpecialFolderLocation function with the
    /// CSIDL_RECENT flag.
    /// </summary>
    OFN_DONTADDTORECENT = 0x02000000,

    /// <summary>Enables the hook function specified in the lpfnHook member.</summary>
    OFN_ENABLEHOOK = 0x00000020,

    /// <summary>
    /// Causes the dialog box to send CDN_INCLUDEITEM notification messages to your OFNHookProc hook procedure when the user opens a
    /// folder. The dialog box sends a notification for each item in the newly opened folder. These messages enable you to control
    /// which items the dialog box displays in the folder's item list.
    /// </summary>
    OFN_ENABLEINCLUDENOTIFY = 0x00400000,

    /// <summary>
    /// Enables the Explorer-style dialog box to be resized using either the mouse or the keyboard. By default, the Explorer-style
    /// Open and Save As dialog boxes allow the dialog box to be resized regardless of whether this flag is set. This flag is
    /// necessary only if you provide a hook procedure or custom template. The old-style dialog box does not permit resizing.
    /// </summary>
    OFN_ENABLESIZING = 0x00800000,

    /// <summary>
    /// The lpTemplateName member is a pointer to the name of a dialog template resource in the module identified by the hInstance
    /// member. If the OFN_EXPLORER flag is set, the system uses the specified template to create a dialog box that is a child of
    /// the default Explorer-style dialog box. If the OFN_EXPLORER flag is not set, the system uses the template to create an
    /// old-style dialog box that replaces the default dialog box.
    /// </summary>
    OFN_ENABLETEMPLATE = 0x00000040,

    /// <summary>
    /// The hInstance member identifies a data block that contains a preloaded dialog box template. The system ignores
    /// lpTemplateName if this flag is specified. If the OFN_EXPLORER flag is set, the system uses the specified template to create
    /// a dialog box that is a child of the default Explorer-style dialog box. If the OFN_EXPLORER flag is not set, the system uses
    /// the template to create an old-style dialog box that replaces the default dialog box.
    /// </summary>
    OFN_ENABLETEMPLATEHANDLE = 0x00000080,

    /// <summary>
    /// Indicates that any customizations made to the Open or Save As dialog box use the Explorer-style customization methods. For
    /// more information, see Explorer-Style Hook Procedures and Explorer-Style Custom Templates.
    /// <para>
    /// By default, the Open and Save As dialog boxes use the Explorer-style user interface regardless of whether this flag is set.
    /// This flag is necessary only if you provide a hook procedure or custom template, or set the OFN_ALLOWMULTISELECT flag.
    /// </para>
    /// <para>
    /// If you want the old-style user interface, omit the OFN_EXPLORER flag and provide a replacement old-style template or hook
    /// procedure. If you want the old style but do not need a custom template or hook procedure, simply provide a hook procedure
    /// that always returns FALSE.
    /// </para>
    /// </summary>
    OFN_EXPLORER = 0x00080000,

    /// <summary>
    /// The user typed a file name extension that differs from the extension specified by lpstrDefExt. The function does not use
    /// this flag if lpstrDefExt is NULL.
    /// </summary>
    OFN_EXTENSIONDIFFERENT = 0x00000400,

    /// <summary>
    /// The user can type only names of existing files in the File Name entry field. If this flag is specified and the user enters
    /// an invalid name, the dialog box procedure displays a warning in a message box. If this flag is specified, the
    /// OFN_PATHMUSTEXIST flag is also used. This flag can be used in an Open dialog box. It cannot be used with a Save As dialog box.
    /// </summary>
    OFN_FILEMUSTEXIST = 0x00001000,

    /// <summary>
    /// Forces the showing of system and hidden files, thus overriding the user setting to show or not show hidden files. However, a
    /// file that is marked both system and hidden is not shown.
    /// </summary>
    OFN_FORCESHOWHIDDEN = 0x10000000,

    /// <summary>Hides the Read Only check box.</summary>
    OFN_HIDEREADONLY = 0x00000004,

    /// <summary>
    /// For old-style dialog boxes, this flag causes the dialog box to use long file names. If this flag is not specified, or if the
    /// OFN_ALLOWMULTISELECT flag is also set, old-style dialog boxes use short file names (8.3 format) for file names with spaces.
    /// Explorer-style dialog boxes ignore this flag and always display long file names.
    /// </summary>
    OFN_LONGNAMES = 0x00200000,

    /// <summary>
    /// Restores the current directory to its original value if the user changed the directory while searching for files.
    /// <para>This flag is ineffective for GetOpenFileName.</para>
    /// </summary>
    OFN_NOCHANGEDIR = 0x00000008,

    /// <summary>
    /// Directs the dialog box to return the path and file name of the selected shortcut (.LNK) file. If this value is not
    /// specified, the dialog box returns the path and file name of the file referenced by the shortcut.
    /// </summary>
    OFN_NODEREFERENCELINKS = 0x00100000,

    /// <summary>
    /// For old-style dialog boxes, this flag causes the dialog box to use short file names (8.3 format). Explorer-style dialog
    /// boxes ignore this flag and always display long file names.
    /// </summary>
    OFN_NOLONGNAMES = 0x00040000,

    /// <summary>Hides and disables the Network button.</summary>
    OFN_NONETWORKBUTTON = 0x00020000,

    /// <summary>The returned file does not have the Read Only check box selected and is not in a write-protected directory.</summary>
    OFN_NOREADONLYRETURN = 0x00008000,

    /// <summary>
    /// The file is not created before the dialog box is closed. This flag should be specified if the application saves the file on
    /// a create-nonmodify network share. When an application specifies this flag, the library does not check for write protection,
    /// a full disk, an open drive door, or network protection. Applications using this flag must perform file operations carefully,
    /// because a file cannot be reopened once it is closed.
    /// </summary>
    OFN_NOTESTFILECREATE = 0x00010000,

    /// <summary>
    /// The common dialog boxes allow invalid characters in the returned file name. Typically, the calling application uses a hook
    /// procedure that checks the file name by using the FILEOKSTRING message. If the text box in the edit control is empty or
    /// contains nothing but spaces, the lists of files and directories are updated. If the text box in the edit control contains
    /// anything else, nFileOffset and nFileExtension are set to values generated by parsing the text. No default extension is added
    /// to the text, nor is text copied to the buffer specified by lpstrFileTitle. If the value specified by nFileOffset is less
    /// than zero, the file name is invalid. Otherwise, the file name is valid, and nFileExtension and nFileOffset can be used as if
    /// the OFN_NOVALIDATE flag had not been specified.
    /// </summary>
    OFN_NOVALIDATE = 0x00000100,

    /// <summary>
    /// Causes the Save As dialog box to generate a message box if the selected file already exists. The user must confirm whether
    /// to overwrite the file.
    /// </summary>
    OFN_OVERWRITEPROMPT = 0x00000002,

    /// <summary>
    /// The user can type only valid paths and file names. If this flag is used and the user types an invalid path and file name in
    /// the File Name entry field, the dialog box function displays a warning in a message box.
    /// </summary>
    OFN_PATHMUSTEXIST = 0x00000800,

    /// <summary>
    /// Causes the Read Only check box to be selected initially when the dialog box is created. This flag indicates the state of the
    /// Read Only check box when the dialog box is closed.
    /// </summary>
    OFN_READONLY = 0x00000001,

    /// <summary>
    /// Specifies that if a call to the OpenFile function fails because of a network sharing violation, the error is ignored and the
    /// dialog box returns the selected file name. If this flag is not set, the dialog box notifies your hook procedure when a
    /// network sharing violation occurs for the file name specified by the user. If you set the OFN_EXPLORER flag, the dialog box
    /// sends the CDN_SHAREVIOLATION message to the hook procedure. If you do not set OFN_EXPLORER, the dialog box sends the
    /// SHAREVISTRING registered message to the hook procedure.
    /// </summary>
    OFN_SHAREAWARE = 0x00004000,

    /// <summary>
    /// Causes the dialog box to display the Help button. The hwndOwner member must specify the window to receive the HELPMSGSTRING
    /// registered messages that the dialog box sends when the user clicks the Help button. An Explorer-style dialog box sends a
    /// CDN_HELP notification message to your hook procedure when the user clicks the Help button.
    /// </summary>
    OFN_SHOWHELP = 0x00000010,
}

/// <summary>A set of bit flags you can use to initialize the dialog box.</summary>
[Flags]
file enum OFN_EX
{
    /// <summary>
    /// If this flag is set, the places bar is not displayed. If this flag is not set, Explorer-style dialog boxes include a places
    /// bar containing icons for commonly-used folders, such as Favorites and Desktop.
    /// </summary>
    OFN_EX_NOPLACESBAR = 0x00000001,
}

/// <summary>
/// <para>
/// [Starting with Windows Vista, the <c>Open</c> and <c>Save As</c> common dialog boxes have been superseded by the Common Item
/// Dialog. We recommended that you use the Common Item Dialog API instead of these dialog boxes from the Common Dialog Box Library.]
/// </para>
/// <para>
/// Receives notification messages sent from the dialog box. The function also receives messages for any additional controls that
/// you defined by specifying a child dialog template. The OFNHookProc hook procedure is an application-defined or library-defined
/// callback function that is used with the Explorer-style <c>Open</c> and <c>Save As</c> dialog boxes.
/// </para>
/// <para>
/// The <c>LPOFNHOOKPROC</c> type defines a pointer to this callback function. OFNHookProc is a placeholder for the
/// application-defined function name.
/// </para>
/// </summary>
/// <param name="Arg1">
/// A handle to the child dialog box of the <c>Open</c> or <c>Save As</c> dialog box. Use the GetParent function to get the handle
/// to the <c>Open</c> or <c>Save As</c> dialog box.
/// </param>
/// <param name="Arg2">The identifier of the message being received.</param>
/// <param name="Arg3">Additional information about the message. The exact meaning depends on the value of the Arg2 parameter.</param>
/// <param name="Arg4">
/// Additional information about the message. The exact meaning depends on the value of the Arg2 parameter. If the Arg2 parameter
/// indicates the WM_INITDIALOG message, Arg4 is a pointer to an OPENFILENAME structure containing the values specified when the
/// dialog box was created.
/// </param>
/// <returns>
/// <para>If the hook procedure returns zero, the default dialog box procedure processes the message.</para>
/// <para>If the hook procedure returns a nonzero value, the default dialog box procedure ignores the message.</para>
/// <para>
/// For the CDN_SHAREVIOLATION and CDN_FILEOK notification messages, the hook procedure should return a nonzero value to indicate
/// that it has used the SetWindowLong function to set a nonzero <c>DWL_MSGRESULT</c> value.
/// </para>
/// </returns>
/// <remarks>
/// <para>
/// If you do not specify the <c>OFN_EXPLORER</c> flag when you create an <c>Open</c> or <c>Save As</c> dialog box, and you want a
/// hook procedure, you must use an old-style OFNHookProcOldStyle hook procedure. In this case, the dialog box will have the
/// old-style user interface.
/// </para>
/// <para>
/// When you use the GetOpenFileName or GetSaveFileName functions to create an Explorer-style <c>Open</c> or <c>Save As</c> dialog
/// box, you can provide an OFNHookProc hook procedure. To enable the hook procedure, use the OPENFILENAME structure that you passed
/// to the dialog creation function. Specify the pointer to the hook procedure in the <c>lpfnHook</c> member and specify the
/// <c>OFN_ENABLEHOOK</c> flag in the <c>Flags</c> member.
/// </para>
/// <para>
/// If you provide a hook procedure for an Explorer-style common dialog box, the system creates a dialog box that is a child of the
/// default dialog box. The hook procedure acts as the dialog procedure for the child dialog. This child dialog is based on the
/// template you specified in the OPENFILENAME structure, or it is a default child dialog if no template is specified. The child
/// dialog is created when the default dialog procedure is processing its WM_INITDIALOG message. After the child dialog processes
/// its own <c>WM_INITDIALOG</c> message, the default dialog procedure moves the standard controls, if necessary, to make room for
/// any additional controls of the child dialog. The system then sends the CDN_INITDONE notification message to the hook procedure.
/// </para>
/// <para>
/// The hook procedure does not receive messages intended for the standard controls of the default dialog box. You can subclass the
/// standard controls, but this is discouraged because it may make your application incompatible with later versions. However, the
/// Explorer-style common dialog boxes provide a set of messages that the hook procedure can use to monitor and control the dialog.
/// These include a set of notification messages sent from the dialog, as well as messages that you can send to retrieve information
/// from the dialog. For a complete list of these messages, see Explorer-Style Hook Procedures.
/// </para>
/// <para>
/// If the hook procedure processes the WM_CTLCOLORDLG message, it must return a valid brush handle to painting the background of
/// the dialog box. In general, if it processes any <c>WM_CTLCOLOR*</c> message, it must return a valid brush handle to painting the
/// background of the specified control.
/// </para>
/// <para>
/// Do not call the EndDialog function from the hook procedure. Instead, the hook procedure can call the PostMessage function to
/// post a WM_COMMAND message with the <c>IDCANCEL</c> value to the dialog box procedure. Posting <c>IDCANCEL</c> closes the dialog
/// box and causes the dialog box function to return <c>FALSE</c>. If you need to know why the hook procedure closed the dialog box,
/// you must provide your own communication mechanism between the hook procedure and your application.
/// </para>
/// </remarks>
// https://docs.microsoft.com/en-us/windows/win32/api/commdlg/nc-commdlg-lpofnhookproc LPOFNHOOKPROC Lpofnhookproc; UINT_PTR
// Lpofnhookproc( HWND Arg1, UINT Arg2, WPARAM Arg3, LPARAM Arg4 ) {...}
[UnmanagedFunctionPointer(CallingConvention.Winapi)]
file delegate IntPtr LPOFNHOOKPROC(IntPtr Arg1, uint Arg2, IntPtr Arg3, IntPtr Arg4);

/// <summary>
/// <para>
/// [Starting with Windows Vista, the <c>Open</c> and <c>Save As</c> common dialog boxes have been superseded by the Common Item
/// Dialog. We recommended that you use the Common Item Dialog API instead of these dialog boxes from the Common Dialog Box Library.]
/// </para>
/// <para>
/// Contains information that the GetOpenFileName and GetSaveFileName functions use to initialize an <c>Open</c> or <c>Save As</c>
/// dialog box. After the user closes the dialog box, the system returns information about the user's selection in this structure.
/// </para>
/// </summary>
/// <remarks>
/// For compatibility reasons, the Places Bar is hidden if <c>Flags</c> is set to <c>OFN_ENABLEHOOK</c> and <c>lStructSize</c> is <c>OPENFILENAME_SIZE_VERSION_400</c>.
/// </remarks>
// https://docs.microsoft.com/en-us/windows/win32/api/commdlg/ns-commdlg-openfilenamea typedef struct tagOFNA { DWORD lStructSize;
// HWND hwndOwner; HINSTANCE hInstance; LPCSTR lpstrFilter; LPSTR lpstrCustomFilter; DWORD nMaxCustFilter; DWORD nFilterIndex; LPSTR
// lpstrFile; DWORD nMaxFile; LPSTR lpstrFileTitle; DWORD nMaxFileTitle; LPCSTR lpstrInitialDir; LPCSTR lpstrTitle; DWORD Flags;
// WORD nFileOffset; WORD nFileExtension; LPCSTR lpstrDefExt; LPARAM lCustData; LPOFNHOOKPROC lpfnHook; LPCSTR lpTemplateName;
// LPEDITMENU lpEditInfo; LPCSTR lpstrPrompt; void *pvReserved; DWORD dwReserved; DWORD FlagsEx; } OPENFILENAMEA, *LPOPENFILENAMEA;
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
file struct OPENFILENAME
{
    /// <summary>
    /// <para>Type: <c>DWORD</c></para>
    /// <para>The length, in bytes, of the structure. Use
    /// <code>sizeof (OPENFILENAME)</code>
    /// for this parameter.
    /// </para>
    /// </summary>
    public uint lStructSize;

    /// <summary>
    /// <para>Type: <c>HWND</c></para>
    /// <para>
    /// A handle to the window that owns the dialog box. This member can be any valid window handle, or it can be <c>NULL</c> if the
    /// dialog box has no owner.
    /// </para>
    /// </summary>
    public IntPtr hwndOwner; // TODO: HWND

    /// <summary>
    /// <para>Type: <c>HINSTANCE</c></para>
    /// <para>
    /// If the <c>OFN_ENABLETEMPLATEHANDLE</c> flag is set in the <c>Flags</c> member, <c>hInstance</c> is a handle to a memory
    /// object containing a dialog box template. If the <c>OFN_ENABLETEMPLATE</c> flag is set, <c>hInstance</c> is a handle to a
    /// module that contains a dialog box template named by the <c>lpTemplateName</c> member. If neither flag is set, this member is
    /// ignored. If the <c>OFN_EXPLORER</c> flag is set, the system uses the specified template to create a dialog box that is a
    /// child of the default Explorer-style dialog box. If the <c>OFN_EXPLORER</c> flag is not set, the system uses the template to
    /// create an old-style dialog box that replaces the default dialog box.
    /// </para>
    /// </summary>
    public IntPtr hInstance; // TODO: HINSTANCE

    /// <summary>
    /// <para>Type: <c>LPCTSTR</c></para>
    /// <para>
    /// A buffer containing pairs of null-terminated filter strings. The last string in the buffer must be terminated by two
    /// <c>NULL</c> characters.
    /// </para>
    /// <para>
    /// The first string in each pair is a display string that describes the filter (for example, "Text Files"), and the second
    /// string specifies the filter pattern (for example, ".TXT"). To specify multiple filter patterns for a single display string,
    /// use a semicolon to separate the patterns (for example, ".TXT;.DOC;.BAK"). A pattern string can be a combination of valid
    /// file name characters and the asterisk (*) wildcard character. Do not include spaces in the pattern string.
    /// </para>
    /// <para>
    /// The system does not change the order of the filters. It displays them in the <c>File Types</c> combo box in the order
    /// specified in <c>lpstrFilter</c>.
    /// </para>
    /// <para>If <c>lpstrFilter</c> is <c>NULL</c>, the dialog box does not display any filters.</para>
    /// <para>
    /// In the case of a shortcut, if no filter is set, GetOpenFileName and GetSaveFileName retrieve the name of the .lnk file, not
    /// its target. This behavior is the same as setting the <c>OFN_NODEREFERENCELINKS</c> flag in the <c>Flags</c> member. To
    /// retrieve a shortcut's target without filtering, use the string
    /// <code>"All Files\0*.*\0\0"</code>
    /// .
    /// </para>
    /// </summary>
    [MarshalAs(UnmanagedType.LPTStr)]
    public string lpstrFilter;

    /// <summary>
    /// <para>Type: <c>LPTSTR</c></para>
    /// <para>
    /// A static buffer that contains a pair of null-terminated filter strings for preserving the filter pattern chosen by the user.
    /// The first string is your display string that describes the custom filter, and the second string is the filter pattern
    /// selected by the user. The first time your application creates the dialog box, you specify the first string, which can be any
    /// nonempty string. When the user selects a file, the dialog box copies the current filter pattern to the second string. The
    /// preserved filter pattern can be one of the patterns specified in the <c>lpstrFilter</c> buffer, or it can be a filter
    /// pattern typed by the user. The system uses the strings to initialize the user-defined file filter the next time the dialog
    /// box is created. If the <c>nFilterIndex</c> member is zero, the dialog box uses the custom filter.
    /// </para>
    /// <para>If this member is <c>NULL</c>, the dialog box does not preserve user-defined filter patterns.</para>
    /// <para>
    /// If this member is not <c>NULL</c>, the value of the <c>nMaxCustFilter</c> member must specify the size, in characters, of
    /// the <c>lpstrCustomFilter</c> buffer.
    /// </para>
    /// </summary>
    public StrPtrAuto lpstrCustomFilter;

    /// <summary>
    /// <para>Type: <c>DWORD</c></para>
    /// <para>
    /// The size, in characters, of the buffer identified by <c>lpstrCustomFilter</c>. This buffer should be at least 40 characters
    /// long. This member is ignored if <c>lpstrCustomFilter</c> is <c>NULL</c> or points to a <c>NULL</c> string.
    /// </para>
    /// </summary>
    public uint nMaxCustFilter;

    /// <summary>
    /// <para>Type: <c>DWORD</c></para>
    /// <para>
    /// The index of the currently selected filter in the <c>File Types</c> control. The buffer pointed to by <c>lpstrFilter</c>
    /// contains pairs of strings that define the filters. The first pair of strings has an index value of 1, the second pair 2, and
    /// so on. An index of zero indicates the custom filter specified by <c>lpstrCustomFilter</c>. You can specify an index on input
    /// to indicate the initial filter description and filter pattern for the dialog box. When the user selects a file,
    /// <c>nFilterIndex</c> returns the index of the currently displayed filter. If <c>nFilterIndex</c> is zero and
    /// <c>lpstrCustomFilter</c> is <c>NULL</c>, the system uses the first filter in the <c>lpstrFilter</c> buffer. If all three
    /// members are zero or <c>NULL</c>, the system does not use any filters and does not show any files in the file list control of
    /// the dialog box.
    /// </para>
    /// </summary>
    public uint nFilterIndex;

    /// <summary>
    /// <para>Type: <c>LPTSTR</c></para>
    /// <para>
    /// The file name used to initialize the <c>File Name</c> edit control. The first character of this buffer must be <c>NULL</c>
    /// if initialization is not necessary. When the GetOpenFileName or GetSaveFileName function returns successfully, this buffer
    /// contains the drive designator, path, file name, and extension of the selected file.
    /// </para>
    /// <para>
    /// If the <c>OFN_ALLOWMULTISELECT</c> flag is set and the user selects multiple files, the buffer contains the current
    /// directory followed by the file names of the selected files. For Explorer-style dialog boxes, the directory and file name
    /// strings are <c>NULL</c> separated, with an extra <c>NULL</c> character after the last file name. For old-style dialog boxes,
    /// the strings are space separated and the function uses short file names for file names with spaces. You can use the
    /// FindFirstFile function to convert between long and short file names. If the user selects only one file, the <c>lpstrFile</c>
    /// string does not have a separator between the path and file name.
    /// </para>
    /// <para>
    /// If the buffer is too small, the function returns <c>FALSE</c> and the CommDlgExtendedError function returns
    /// <c>FNERR_BUFFERTOOSMALL</c>. In this case, the first two bytes of the <c>lpstrFile</c> buffer contain the required size, in
    /// bytes or characters.
    /// </para>
    /// </summary>
    public StrPtrAuto lpstrFile;

    /// <summary>
    /// <para>Type: <c>DWORD</c></para>
    /// <para>
    /// The size, in characters, of the buffer pointed to by <c>lpstrFile</c>. The buffer must be large enough to store the path and
    /// file name string or strings, including the terminating <c>NULL</c> character. The GetOpenFileName and GetSaveFileName
    /// functions return <c>FALSE</c> if the buffer is too small to contain the file information. The buffer should be at least 256
    /// characters long.
    /// </para>
    /// </summary>
    public uint nMaxFile;

    /// <summary>
    /// <para>Type: <c>LPTSTR</c></para>
    /// <para>The file name and extension (without path information) of the selected file. This member can be <c>NULL</c>.</para>
    /// </summary>
    public StrPtrAuto lpstrFileTitle;

    /// <summary>
    /// <para>Type: <c>DWORD</c></para>
    /// <para>
    /// The size, in characters, of the buffer pointed to by <c>lpstrFileTitle</c>. This member is ignored if <c>lpstrFileTitle</c>
    /// is <c>NULL</c>.
    /// </para>
    /// </summary>
    public uint nMaxFileTitle;

    /// <summary>
    /// <para>Type: <c>LPCTSTR</c></para>
    /// <para>The initial directory. The algorithm for selecting the initial directory varies on different platforms.</para>
    /// <para><c>Windows 7:</c></para>
    /// <list type="number">
    /// <item>
    /// <term>
    /// If <c>lpstrInitialDir</c> has the same value as was passed the first time the application used an <c>Open</c> or <c>Save
    /// As</c> dialog box, the path most recently selected by the user is used as the initial directory.
    /// </term>
    /// </item>
    /// <item>
    /// <term>Otherwise, if <c>lpstrFile</c> contains a path, that path is the initial directory.</term>
    /// </item>
    /// <item>
    /// <term>Otherwise, if <c>lpstrInitialDir</c> is not <c>NULL</c>, it specifies the initial directory.</term>
    /// </item>
    /// <item>
    /// <term>
    /// If <c>lpstrInitialDir</c> is <c>NULL</c> and the current directory contains any files of the specified filter types, the
    /// initial directory is the current directory.
    /// </term>
    /// </item>
    /// <item>
    /// <term>Otherwise, the initial directory is the personal files directory of the current user.</term>
    /// </item>
    /// <item>
    /// <term>Otherwise, the initial directory is the Desktop folder.</term>
    /// </item>
    /// </list>
    /// <para>Windows 2000/XP/Vista:</para>
    /// <list type="number">
    /// <item>
    /// <term>If <c>lpstrFile</c> contains a path, that path is the initial directory.</term>
    /// </item>
    /// <item>
    /// <term>Otherwise, <c>lpstrInitialDir</c> specifies the initial directory.</term>
    /// </item>
    /// <item>
    /// <term>
    /// Otherwise, if the application has used an <c>Open</c> or <c>Save As</c> dialog box in the past, the path most recently used
    /// is selected as the initial directory. However, if an application is not run for a long time, its saved selected path is discarded.
    /// </term>
    /// </item>
    /// <item>
    /// <term>
    /// If <c>lpstrInitialDir</c> is <c>NULL</c> and the current directory contains any files of the specified filter types, the
    /// initial directory is the current directory.
    /// </term>
    /// </item>
    /// <item>
    /// <term>Otherwise, the initial directory is the personal files directory of the current user.</term>
    /// </item>
    /// <item>
    /// <term>Otherwise, the initial directory is the Desktop folder.</term>
    /// </item>
    /// </list>
    /// </summary>
    public StrPtrAuto lpstrInitialDir;

    /// <summary>
    /// <para>Type: <c>LPCTSTR</c></para>
    /// <para>
    /// A string to be placed in the title bar of the dialog box. If this member is <c>NULL</c>, the system uses the default title
    /// (that is, <c>Save As</c> or <c>Open</c>).
    /// </para>
    /// </summary>
    public StrPtrAuto lpstrTitle;

    /// <summary>
    /// <para>Type: <c>DWORD</c></para>
    /// <para>
    /// A set of bit flags you can use to initialize the dialog box. When the dialog box returns, it sets these flags to indicate
    /// the user's input. This member can be a combination of the following flags.
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <term>Meaning</term>
    /// </listheader>
    /// <item>
    /// <term>OFN_ALLOWMULTISELECT 0x00000200</term>
    /// <term>
    /// The File Name list box allows multiple selections. If you also set the OFN_EXPLORER flag, the dialog box uses the
    /// Explorer-style user interface; otherwise, it uses the old-style user interface. If the user selects more than one file, the
    /// lpstrFile buffer returns the path to the current directory followed by the file names of the selected files. The nFileOffset
    /// member is the offset, in bytes or characters, to the first file name, and the nFileExtension member is not used. For
    /// Explorer-style dialog boxes, the directory and file name strings are NULL separated, with an extra NULL character after the
    /// last file name. This format enables the Explorer-style dialog boxes to return long file names that include spaces. For
    /// old-style dialog boxes, the directory and file name strings are separated by spaces and the function uses short file names
    /// for file names with spaces. You can use the FindFirstFile function to convert between long and short file names. If you
    /// specify a custom template for an old-style dialog box, the definition of the File Name list box must contain the
    /// LBS_EXTENDEDSEL value.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_CREATEPROMPT 0x00002000</term>
    /// <term>
    /// If the user specifies a file that does not exist, this flag causes the dialog box to prompt the user for permission to
    /// create the file. If the user chooses to create the file, the dialog box closes and the function returns the specified name;
    /// otherwise, the dialog box remains open. If you use this flag with the OFN_ALLOWMULTISELECT flag, the dialog box allows the
    /// user to specify only one nonexistent file.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_DONTADDTORECENT 0x02000000</term>
    /// <term>
    /// Prevents the system from adding a link to the selected file in the file system directory that contains the user's most
    /// recently used documents. To retrieve the location of this directory, call the SHGetSpecialFolderLocation function with the
    /// CSIDL_RECENT flag.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_ENABLEHOOK 0x00000020</term>
    /// <term>Enables the hook function specified in the lpfnHook member.</term>
    /// </item>
    /// <item>
    /// <term>OFN_ENABLEINCLUDENOTIFY 0x00400000</term>
    /// <term>
    /// Causes the dialog box to send CDN_INCLUDEITEM notification messages to your OFNHookProc hook procedure when the user opens a
    /// folder. The dialog box sends a notification for each item in the newly opened folder. These messages enable you to control
    /// which items the dialog box displays in the folder's item list.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_ENABLESIZING 0x00800000</term>
    /// <term>
    /// Enables the Explorer-style dialog box to be resized using either the mouse or the keyboard. By default, the Explorer-style
    /// Open and Save As dialog boxes allow the dialog box to be resized regardless of whether this flag is set. This flag is
    /// necessary only if you provide a hook procedure or custom template. The old-style dialog box does not permit resizing.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_ENABLETEMPLATE 0x00000040</term>
    /// <term>
    /// The lpTemplateName member is a pointer to the name of a dialog template resource in the module identified by the hInstance
    /// member. If the OFN_EXPLORER flag is set, the system uses the specified template to create a dialog box that is a child of
    /// the default Explorer-style dialog box. If the OFN_EXPLORER flag is not set, the system uses the template to create an
    /// old-style dialog box that replaces the default dialog box.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_ENABLETEMPLATEHANDLE 0x00000080</term>
    /// <term>
    /// The hInstance member identifies a data block that contains a preloaded dialog box template. The system ignores
    /// lpTemplateName if this flag is specified. If the OFN_EXPLORER flag is set, the system uses the specified template to create
    /// a dialog box that is a child of the default Explorer-style dialog box. If the OFN_EXPLORER flag is not set, the system uses
    /// the template to create an old-style dialog box that replaces the default dialog box.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_EXPLORER 0x00080000</term>
    /// <term>
    /// Indicates that any customizations made to the Open or Save As dialog box use the Explorer-style customization methods. For
    /// more information, see Explorer-Style Hook Procedures and Explorer-Style Custom Templates. By default, the Open and Save As
    /// dialog boxes use the Explorer-style user interface regardless of whether this flag is set. This flag is necessary only if
    /// you provide a hook procedure or custom template, or set the OFN_ALLOWMULTISELECT flag. If you want the old-style user
    /// interface, omit the OFN_EXPLORER flag and provide a replacement old-style template or hook procedure. If you want the old
    /// style but do not need a custom template or hook procedure, simply provide a hook procedure that always returns FALSE.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_EXTENSIONDIFFERENT 0x00000400</term>
    /// <term>
    /// The user typed a file name extension that differs from the extension specified by lpstrDefExt. The function does not use
    /// this flag if lpstrDefExt is NULL.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_FILEMUSTEXIST 0x00001000</term>
    /// <term>
    /// The user can type only names of existing files in the File Name entry field. If this flag is specified and the user enters
    /// an invalid name, the dialog box procedure displays a warning in a message box. If this flag is specified, the
    /// OFN_PATHMUSTEXIST flag is also used. This flag can be used in an Open dialog box. It cannot be used with a Save As dialog box.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_FORCESHOWHIDDEN 0x10000000</term>
    /// <term>
    /// Forces the showing of system and hidden files, thus overriding the user setting to show or not show hidden files. However, a
    /// file that is marked both system and hidden is not shown.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_HIDEREADONLY 0x00000004</term>
    /// <term>Hides the Read Only check box.</term>
    /// </item>
    /// <item>
    /// <term>OFN_LONGNAMES 0x00200000</term>
    /// <term>
    /// For old-style dialog boxes, this flag causes the dialog box to use long file names. If this flag is not specified, or if the
    /// OFN_ALLOWMULTISELECT flag is also set, old-style dialog boxes use short file names (8.3 format) for file names with spaces.
    /// Explorer-style dialog boxes ignore this flag and always display long file names.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_NOCHANGEDIR 0x00000008</term>
    /// <term>
    /// Restores the current directory to its original value if the user changed the directory while searching for files. This flag
    /// is ineffective for GetOpenFileName.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_NODEREFERENCELINKS 0x00100000</term>
    /// <term>
    /// Directs the dialog box to return the path and file name of the selected shortcut (.LNK) file. If this value is not
    /// specified, the dialog box returns the path and file name of the file referenced by the shortcut.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_NOLONGNAMES 0x00040000</term>
    /// <term>
    /// For old-style dialog boxes, this flag causes the dialog box to use short file names (8.3 format). Explorer-style dialog
    /// boxes ignore this flag and always display long file names.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_NONETWORKBUTTON 0x00020000</term>
    /// <term>Hides and disables the Network button.</term>
    /// </item>
    /// <item>
    /// <term>OFN_NOREADONLYRETURN 0x00008000</term>
    /// <term>The returned file does not have the Read Only check box selected and is not in a write-protected directory.</term>
    /// </item>
    /// <item>
    /// <term>OFN_NOTESTFILECREATE 0x00010000</term>
    /// <term>
    /// The file is not created before the dialog box is closed. This flag should be specified if the application saves the file on
    /// a create-nonmodify network share. When an application specifies this flag, the library does not check for write protection,
    /// a full disk, an open drive door, or network protection. Applications using this flag must perform file operations carefully,
    /// because a file cannot be reopened once it is closed.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_NOVALIDATE 0x00000100</term>
    /// <term>
    /// The common dialog boxes allow invalid characters in the returned file name. Typically, the calling application uses a hook
    /// procedure that checks the file name by using the FILEOKSTRING message. If the text box in the edit control is empty or
    /// contains nothing but spaces, the lists of files and directories are updated. If the text box in the edit control contains
    /// anything else, nFileOffset and nFileExtension are set to values generated by parsing the text. No default extension is added
    /// to the text, nor is text copied to the buffer specified by lpstrFileTitle. If the value specified by nFileOffset is less
    /// than zero, the file name is invalid. Otherwise, the file name is valid, and nFileExtension and nFileOffset can be used as if
    /// the OFN_NOVALIDATE flag had not been specified.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_OVERWRITEPROMPT 0x00000002</term>
    /// <term>
    /// Causes the Save As dialog box to generate a message box if the selected file already exists. The user must confirm whether
    /// to overwrite the file.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_PATHMUSTEXIST 0x00000800</term>
    /// <term>
    /// The user can type only valid paths and file names. If this flag is used and the user types an invalid path and file name in
    /// the File Name entry field, the dialog box function displays a warning in a message box.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_READONLY 0x00000001</term>
    /// <term>
    /// Causes the Read Only check box to be selected initially when the dialog box is created. This flag indicates the state of the
    /// Read Only check box when the dialog box is closed.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_SHAREAWARE 0x00004000</term>
    /// <term>
    /// Specifies that if a call to the OpenFile function fails because of a network sharing violation, the error is ignored and the
    /// dialog box returns the selected file name. If this flag is not set, the dialog box notifies your hook procedure when a
    /// network sharing violation occurs for the file name specified by the user. If you set the OFN_EXPLORER flag, the dialog box
    /// sends the CDN_SHAREVIOLATION message to the hook procedure. If you do not set OFN_EXPLORER, the dialog box sends the
    /// SHAREVISTRING registered message to the hook procedure.
    /// </term>
    /// </item>
    /// <item>
    /// <term>OFN_SHOWHELP 0x00000010</term>
    /// <term>
    /// Causes the dialog box to display the Help button. The hwndOwner member must specify the window to receive the HELPMSGSTRING
    /// registered messages that the dialog box sends when the user clicks the Help button. An Explorer-style dialog box sends a
    /// CDN_HELP notification message to your hook procedure when the user clicks the Help button.
    /// </term>
    /// </item>
    /// </list>
    /// </summary>
    public OFN Flags;

    /// <summary>
    /// <para>Type: <c>WORD</c></para>
    /// <para>
    /// The zero-based offset, in characters, from the beginning of the path to the file name in the string pointed to by
    /// <c>lpstrFile</c>. For the ANSI version, this is the number of bytes; for the Unicode version, this is the number of
    /// characters. For example, if <c>lpstrFile</c> points to the following string, "c:\dir1\dir2\file.ext", this member contains
    /// the value 13 to indicate the offset of the "file.ext" string. If the user selects more than one file, <c>nFileOffset</c> is
    /// the offset to the first file name.
    /// </para>
    /// </summary>
    public ushort nFileOffset;

    /// <summary>
    /// <para>Type: <c>WORD</c></para>
    /// <para>
    /// The zero-based offset, in characters, from the beginning of the path to the file name extension in the string pointed to by
    /// <c>lpstrFile</c>. For the ANSI version, this is the number of bytes; for the Unicode version, this is the number of
    /// characters. Usually the file name extension is the substring which follows the last occurrence of the dot (".") character.
    /// For example, txt is the extension of the filename readme.txt, html the extension of readme.txt.html. Therefore, if
    /// <c>lpstrFile</c> points to the string "c:\dir1\dir2\readme.txt", this member contains the value 20. If <c>lpstrFile</c>
    /// points to the string "c:\dir1\dir2\readme.txt.html", this member contains the value 24. If <c>lpstrFile</c> points to the
    /// string "c:\dir1\dir2\readme.txt.html.", this member contains the value 29. If <c>lpstrFile</c> points to a string that does
    /// not contain any "." character such as "c:\dir1\dir2\readme", this member contains zero.
    /// </para>
    /// </summary>
    public ushort nFileExtension;

    /// <summary>
    /// <para>Type: <c>LPCTSTR</c></para>
    /// <para>
    /// The default extension. GetOpenFileName and GetSaveFileName append this extension to the file name if the user fails to type
    /// an extension. This string can be any length, but only the first three characters are appended. The string should not contain
    /// a period (.). If this member is <c>NULL</c> and the user fails to type an extension, no extension is appended.
    /// </para>
    /// </summary>
    public StrPtrAuto lpstrDefExt;

    /// <summary>
    /// <para>Type: <c>LPARAM</c></para>
    /// <para>
    /// Application-defined data that the system passes to the hook procedure identified by the <c>lpfnHook</c> member. When the
    /// system sends the WM_INITDIALOG message to the hook procedure, the message's lParam parameter is a pointer to the
    /// <c>OPENFILENAME</c> structure specified when the dialog box was created. The hook procedure can use this pointer to get the
    /// <c>lCustData</c> value.
    /// </para>
    /// </summary>
    public IntPtr lCustData;

    /// <summary>
    /// <para>Type: <c>LPOFNHOOKPROC</c></para>
    /// <para>
    /// A pointer to a hook procedure. This member is ignored unless the <c>Flags</c> member includes the <c>OFN_ENABLEHOOK</c> flag.
    /// </para>
    /// <para>
    /// If the <c>OFN_EXPLORER</c> flag is not set in the <c>Flags</c> member, <c>lpfnHook</c> is a pointer to an
    /// OFNHookProcOldStyle hook procedure that receives messages intended for the dialog box. The hook procedure returns
    /// <c>FALSE</c> to pass a message to the default dialog box procedure or <c>TRUE</c> to discard the message.
    /// </para>
    /// <para>
    /// If <c>OFN_EXPLORER</c> is set, <c>lpfnHook</c> is a pointer to an OFNHookProc hook procedure. The hook procedure receives
    /// notification messages sent from the dialog box. The hook procedure also receives messages for any additional controls that
    /// you defined by specifying a child dialog template. The hook procedure does not receive messages intended for the standard
    /// controls of the default dialog box.
    /// </para>
    /// </summary>
    [MarshalAs(UnmanagedType.FunctionPtr)]
    public LPOFNHOOKPROC lpfnHook;

    /// <summary>
    /// <para>Type: <c>LPCTSTR</c></para>
    /// <para>
    /// The name of the dialog template resource in the module identified by the <c>hInstance</c> member. For numbered dialog box
    /// resources, this can be a value returned by the MAKEINTRESOURCE macro. This member is ignored unless the
    /// <c>OFN_ENABLETEMPLATE</c> flag is set in the <c>Flags</c> member. If the <c>OFN_EXPLORER</c> flag is set, the system uses
    /// the specified template to create a dialog box that is a child of the default Explorer-style dialog box. If the
    /// <c>OFN_EXPLORER</c> flag is not set, the system uses the template to create an old-style dialog box that replaces the
    /// default dialog box.
    /// </para>
    /// </summary>
    [MarshalAs(UnmanagedType.LPTStr)]
    public string lpTemplateName;

    /// <summary>
    /// <para>Type: <c>void*</c></para>
    /// <para>This member is reserved.</para>
    /// </summary>
    private IntPtr pvReserved;

    /// <summary>
    /// <para>Type: <c>DWORD</c></para>
    /// <para>This member is reserved.</para>
    /// </summary>
    private uint dwReserved;

    /// <summary>
    /// <para>Type: <c>DWORD</c></para>
    /// <para>A set of bit flags you can use to initialize the dialog box. Currently, this member can be zero or the following flag.</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <term>Meaning</term>
    /// </listheader>
    /// <item>
    /// <term>OFN_EX_NOPLACESBAR 0x00000001</term>
    /// <term>
    /// If this flag is set, the places bar is not displayed. If this flag is not set, Explorer-style dialog boxes include a places
    /// bar containing icons for commonly-used folders, such as Favorites and Desktop.
    /// </term>
    /// </item>
    /// </list>
    /// </summary>
    public OFN_EX FlagsEx;
}

file class PInvoke
{
    /// <summary>
    /// <para>
    /// [Starting with Windows Vista, the <c>Open</c> and <c>Save As</c> common dialog boxes have been superseded by the Common Item
    /// Dialog. We recommended that you use the Common Item Dialog API instead of these dialog boxes from the Common Dialog Box Library.]
    /// </para>
    /// <para>
    /// Creates an <c>Open</c> dialog box that lets the user specify the drive, directory, and the name of a file or set of files to be opened.
    /// </para>
    /// </summary>
    /// <param name="Arg1">
    /// <para>Type: <c>LPOPENFILENAME</c></para>
    /// <para>
    /// A pointer to an OPENFILENAME structure that contains information used to initialize the dialog box. When <c>GetOpenFileName</c>
    /// returns, this structure contains information about the user's file selection.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>Type: <c>BOOL</c></para>
    /// <para>
    /// If the user specifies a file name and clicks the <c>OK</c> button, the return value is nonzero. The buffer pointed to by the
    /// <c>lpstrFile</c> member of the OPENFILENAME structure contains the full path and file name specified by the user.
    /// </para>
    /// <para>
    /// If the user cancels or closes the <c>Open</c> dialog box or an error occurs, the return value is zero. To get extended error
    /// information, call the CommDlgExtendedError function, which can return one of the following values.
    /// </para>
    /// </returns>
    /// <remarks>
    /// <para>
    /// The Explorer-style <c>Open</c> dialog box provides user-interface features that are similar to the Windows Explorer. You can
    /// provide an OFNHookProc hook procedure for an Explorer-style <c>Open</c> dialog box. To enable the hook procedure, set the
    /// <c>OFN_EXPLORER</c> and <c>OFN_ENABLEHOOK</c> flags in the <c>Flags</c> member of the OPENFILENAME structure and specify the
    /// address of the hook procedure in the <c>lpfnHook</c> member.
    /// </para>
    /// <para>
    /// Windows continues to support the old-style <c>Open</c> dialog box for applications that want to maintain a user-interface
    /// consistent with the old-style user-interface. To display the old-style <c>Open</c> dialog box, enable an OFNHookProcOldStyle
    /// hook procedure and ensure that the <c>OFN_EXPLORER</c> flag is not set.
    /// </para>
    /// <para>To display a dialog box that allows the user to select a directory instead of a file, call the SHBrowseForFolder function.</para>
    /// <para>Note, when selecting multiple files, the total character limit for the file names depends on the version of the function.</para>
    /// <list type="bullet">
    /// <item>
    /// <term>ANSI: 32k limit</term>
    /// </item>
    /// <item>
    /// <term>Unicode: no restriction</term>
    /// </item>
    /// </list>
    /// <para>Examples</para>
    /// <para>For an example, see Opening a File.</para>
    /// </remarks>
    // https://docs.microsoft.com/en-us/windows/win32/api/commdlg/nf-commdlg-getopenfilenamea BOOL GetOpenFileNameA( LPOPENFILENAMEA
    // Arg1 );
    [DllImport("comdlg32.dll", SetLastError = false, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetOpenFileName(ref OPENFILENAME Arg1);

    /// <summary>
    /// <para>
    /// [Starting with Windows Vista, the <c>Open</c> and <c>Save As</c> common dialog boxes have been superseded by the Common Item
    /// Dialog. We recommended that you use the Common Item Dialog API instead of these dialog boxes from the Common Dialog Box Library.]
    /// </para>
    /// <para>Creates a <c>Save</c> dialog box that lets the user specify the drive, directory, and name of a file to save.</para>
    /// </summary>
    /// <param name="Arg1">
    /// <para>Type: <c>LPOPENFILENAME</c></para>
    /// <para>
    /// A pointer to an OPENFILENAME structure that contains information used to initialize the dialog box. When <c>GetSaveFileName</c>
    /// returns, this structure contains information about the user's file selection.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>Type: <c>BOOL</c></para>
    /// <para>
    /// If the user specifies a file name and clicks the <c>OK</c> button and the function is successful, the return value is nonzero.
    /// The buffer pointed to by the <c>lpstrFile</c> member of the OPENFILENAME structure contains the full path and file name
    /// specified by the user.
    /// </para>
    /// <para>
    /// If the user cancels or closes the <c>Save</c> dialog box or an error such as the file name buffer being too small occurs, the
    /// return value is zero. To get extended error information, call the CommDlgExtendedError function, which can return one of the
    /// following values:
    /// </para>
    /// </returns>
    /// <remarks>
    /// <para>
    /// The Explorer-style <c>Save</c> dialog box that provides user-interface features that are similar to the Windows Explorer. You
    /// can provide an OFNHookProc hook procedure for an Explorer-style <c>Save</c> dialog box. To enable the hook procedure, set the
    /// <c>OFN_EXPLORER</c> and <c>OFN_ENABLEHOOK</c> flags in the <c>Flags</c> member of the OPENFILENAME structure and specify the
    /// address of the hook procedure in the <c>lpfnHook</c> member.
    /// </para>
    /// <para>
    /// Windows continues to support old-style <c>Save</c> dialog boxes for applications that want to maintain a user-interface
    /// consistent with the old-style user-interface. To display the old-style <c>Save</c> dialog box, enable an OFNHookProcOldStyle
    /// hook procedure and ensure that the <c>OFN_EXPLORER</c> flag is not set.
    /// </para>
    /// <para>Examples</para>
    /// <para>For an example, see Creating an Enhanced Metafile.</para>
    /// </remarks>
    // https://docs.microsoft.com/en-us/windows/win32/api/commdlg/nf-commdlg-getsavefilenamea BOOL GetSaveFileNameA( LPOPENFILENAMEA
    // Arg1 );
    [DllImport("comdlg32.dll", SetLastError = false, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetSaveFileName(ref OPENFILENAME Arg1);

    /// <summary>
    /// Returns a common dialog box error code. This code indicates the most recent error to occur during the execution of one of the
    /// common dialog box functions.
    /// </summary>
    /// <returns>
    /// <para>Type: <c>DWORD</c></para>
    /// <para>
    /// If the most recent call to a common dialog box function succeeded, the return value is undefined. If the common dialog box
    /// function returned <c>FALSE</c> because the user closed or canceled the dialog box, the return value is zero. Otherwise, the
    /// return value is a nonzero error code.
    /// </para>
    /// <para>
    /// The <c>CommDlgExtendedError</c> function can return general error codes for any of the common dialog box functions. In addition,
    /// there are error codes that are returned only for a specific common dialog box. All of these error codes are defined in Cderr.h.
    /// The following general error codes can be returned for any of the common dialog box functions.
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Return code/value</term>
    /// <term>Description</term>
    /// </listheader>
    /// <item>
    /// <term>CDERR_DIALOGFAILURE 0xFFFF</term>
    /// <term>
    /// The dialog box could not be created. The common dialog box function's call to the DialogBox function failed. For example, this
    /// error occurs if the common dialog box call specifies an invalid window handle.
    /// </term>
    /// </item>
    /// <item>
    /// <term>CDERR_FINDRESFAILURE 0x0006</term>
    /// <term>The common dialog box function failed to find a specified resource.</term>
    /// </item>
    /// <item>
    /// <term>CDERR_INITIALIZATION 0x0002</term>
    /// <term>The common dialog box function failed during initialization. This error often occurs when sufficient memory is not available.</term>
    /// </item>
    /// <item>
    /// <term>CDERR_LOADRESFAILURE 0x0007</term>
    /// <term>The common dialog box function failed to load a specified resource.</term>
    /// </item>
    /// <item>
    /// <term>CDERR_LOADSTRFAILURE 0x0005</term>
    /// <term>The common dialog box function failed to load a specified string.</term>
    /// </item>
    /// <item>
    /// <term>CDERR_LOCKRESFAILURE 0x0008</term>
    /// <term>The common dialog box function failed to lock a specified resource.</term>
    /// </item>
    /// <item>
    /// <term>CDERR_MEMALLOCFAILURE 0x0009</term>
    /// <term>The common dialog box function was unable to allocate memory for internal structures.</term>
    /// </item>
    /// <item>
    /// <term>CDERR_MEMLOCKFAILURE 0x000A</term>
    /// <term>The common dialog box function was unable to lock the memory associated with a handle.</term>
    /// </item>
    /// <item>
    /// <term>CDERR_NOHINSTANCE 0x0004</term>
    /// <term>
    /// The ENABLETEMPLATE flag was set in the Flags member of the initialization structure for the corresponding common dialog box, but
    /// you failed to provide a corresponding instance handle.
    /// </term>
    /// </item>
    /// <item>
    /// <term>CDERR_NOHOOK 0x000B</term>
    /// <term>
    /// The ENABLEHOOK flag was set in the Flags member of the initialization structure for the corresponding common dialog box, but you
    /// failed to provide a pointer to a corresponding hook procedure.
    /// </term>
    /// </item>
    /// <item>
    /// <term>CDERR_NOTEMPLATE 0x0003</term>
    /// <term>
    /// The ENABLETEMPLATE flag was set in the Flags member of the initialization structure for the corresponding common dialog box, but
    /// you failed to provide a corresponding template.
    /// </term>
    /// </item>
    /// <item>
    /// <term>CDERR_REGISTERMSGFAIL 0x000C</term>
    /// <term>The RegisterWindowMessage function returned an error code when it was called by the common dialog box function.</term>
    /// </item>
    /// <item>
    /// <term>CDERR_STRUCTSIZE 0x0001</term>
    /// <term>The lStructSize member of the initialization structure for the corresponding common dialog box is invalid.</term>
    /// </item>
    /// </list>
    /// <para>The following error codes can be returned for the PrintDlg function.</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Return code/value</term>
    /// <term>Description</term>
    /// </listheader>
    /// <item>
    /// <term>PDERR_CREATEICFAILURE 0x100A</term>
    /// <term>The PrintDlg function failed when it attempted to create an information context.</term>
    /// </item>
    /// <item>
    /// <term>PDERR_DEFAULTDIFFERENT 0x100C</term>
    /// <term>
    /// You called the PrintDlg function with the DN_DEFAULTPRN flag specified in the wDefault member of the DEVNAMES structure, but the
    /// printer described by the other structure members did not match the current default printer. This error occurs when you store the
    /// DEVNAMES structure, and the user changes the default printer by using the Control Panel. To use the printer described by the
    /// DEVNAMES structure, clear the DN_DEFAULTPRN flag and call PrintDlg again. To use the default printer, replace the DEVNAMES
    /// structure (and the structure, if one exists) with NULL; and call PrintDlg again.
    /// </term>
    /// </item>
    /// <item>
    /// <term>PDERR_DNDMMISMATCH 0x1009</term>
    /// <term>The data in the DEVMODE and DEVNAMES structures describes two different printers.</term>
    /// </item>
    /// <item>
    /// <term>PDERR_GETDEVMODEFAIL 0x1005</term>
    /// <term>The printer driver failed to initialize a DEVMODE structure.</term>
    /// </item>
    /// <item>
    /// <term>PDERR_INITFAILURE 0x1006</term>
    /// <term>
    /// The PrintDlg function failed during initialization, and there is no more specific extended error code to describe the failure.
    /// This is the generic default error code for the function.
    /// </term>
    /// </item>
    /// <item>
    /// <term>PDERR_LOADDRVFAILURE 0x1004</term>
    /// <term>The PrintDlg function failed to load the device driver for the specified printer.</term>
    /// </item>
    /// <item>
    /// <term>PDERR_NODEFAULTPRN 0x1008</term>
    /// <term>A default printer does not exist.</term>
    /// </item>
    /// <item>
    /// <term>PDERR_NODEVICES 0x1007</term>
    /// <term>No printer drivers were found.</term>
    /// </item>
    /// <item>
    /// <term>PDERR_PARSEFAILURE 0x1002</term>
    /// <term>The PrintDlg function failed to parse the strings in the [devices] section of the WIN.INI file.</term>
    /// </item>
    /// <item>
    /// <term>PDERR_PRINTERNOTFOUND 0x100B</term>
    /// <term>The [devices] section of the WIN.INI file did not contain an entry for the requested printer.</term>
    /// </item>
    /// <item>
    /// <term>PDERR_RETDEFFAILURE 0x1003</term>
    /// <term>
    /// The PD_RETURNDEFAULT flag was specified in the Flags member of the PRINTDLG structure, but the hDevMode or hDevNames member was
    /// not NULL.
    /// </term>
    /// </item>
    /// <item>
    /// <term>PDERR_SETUPFAILURE 0x1001</term>
    /// <term>The PrintDlg function failed to load the required resources.</term>
    /// </item>
    /// </list>
    /// <para>The following error codes can be returned for the ChooseFont function.</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Return code/value</term>
    /// <term>Description</term>
    /// </listheader>
    /// <item>
    /// <term>CFERR_MAXLESSTHANMIN CFERR_MAXLESSTHANMIN</term>
    /// <term>
    /// The size specified in the nSizeMax member of the CHOOSEFONT structure is less than the size specified in the nSizeMin member.
    /// </term>
    /// </item>
    /// <item>
    /// <term>CFERR_NOFONTS 0x2001</term>
    /// <term>No fonts exist.</term>
    /// </item>
    /// </list>
    /// <para>The following error codes can be returned for the GetOpenFileName and GetSaveFileName functions.</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Return code/value</term>
    /// <term>Description</term>
    /// </listheader>
    /// <item>
    /// <term>FNERR_BUFFERTOOSMALL 0x3003</term>
    /// <term>
    /// The buffer pointed to by the lpstrFile member of the OPENFILENAME structure is too small for the file name specified by the
    /// user. The first two bytes of the lpstrFile buffer contain an integer value specifying the size required to receive the full
    /// name, in characters.
    /// </term>
    /// </item>
    /// <item>
    /// <term>FNERR_INVALIDFILENAME 0x3002</term>
    /// <term>A file name is invalid.</term>
    /// </item>
    /// <item>
    /// <term>FNERR_SUBCLASSFAILURE 0x3001</term>
    /// <term>An attempt to subclass a list box failed because sufficient memory was not available.</term>
    /// </item>
    /// </list>
    /// <para>The following error code can be returned for the FindText and ReplaceText functions.</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Return code/value</term>
    /// <term>Description</term>
    /// </listheader>
    /// <item>
    /// <term>FRERR_BUFFERLENGTHZERO 0x4001</term>
    /// <term>A member of the FINDREPLACE structure points to an invalid buffer.</term>
    /// </item>
    /// </list>
    /// </returns>
    // https://docs.microsoft.com/en-us/windows/win32/api/commdlg/nf-commdlg-commdlgextendederror DWORD CommDlgExtendedError();
    [DllImport("comdlg32.dll", SetLastError = false, ExactSpelling = true)]
    public static extern ERR CommDlgExtendedError();
}

internal abstract class FileDialog
{
    public const int MAX_FILE_LENGTH = 2048;

    /// <summary>
    ///  Specifies that the user can type only valid paths and file names. If this flag is
    ///  used and the user types an invalid path and file name in the File Name entry field, 
    ///  a warning is displayed in a message box.
    /// </summary>
    public bool CheckPathExists { get; set; } = false; // OFN_PATHMUSTEXIST

    /// <summary>
    ///       Gets or sets the current file name filter string,
    ///       which determines the choices that appear in the "Save as file type" or
    ///       "Files of type" box at the bottom of the dialog box.
    ///
    ///       This is an example filter string:
    ///       Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*"
    /// </summary>
    /// <exception cref="System.ArgumentException">
    ///  Thrown in the setter if the new filter string does not have an even number of tokens
    ///  separated by the vertical bar character '|' (that is, the new filter string is invalid.)
    /// </exception>
    /// <remarks>
    ///  If DereferenceLinks is true and the filter string is null, a blank
    ///  filter string (equivalent to "|*.*") will be automatically substituted to work
    ///  around the issue documented in Knowledge Base article 831559
    ///     Callers must have FileIOPermission(PermissionState.Unrestricted) to call this API.
    /// </remarks>
    public string? Filter { get; set; } = "All files(*.*)\0\0";

    /// <summary>
    ///  Gets or sets the index of the filter currently selected in the file dialog box.
    ///
    ///  NOTE:  The index of the first filter entry is 1, not 0.  
    /// </summary>
    public int FilterIndex { get; set; } = 1;

    /// <summary>
    ///  Gets or sets the initial directory displayed by the file dialog box.
    /// </summary>
    public string? InitialDirectory { get; set; } = null;

    /// <summary>
    ///       Gets or sets a string shown in the title bar of the file dialog.
    ///       If this property is null, a localized default from the operating
    ///       system itself will be used (typically something like "Save As" or "Open")
    /// </summary>
    public string? Title { get; set; } = "Open a file...";

    /// <summary>
    ///  Gets or sets a value indicating whether the dialog box accepts only valid
    ///  Win32 file names.
    /// </summary>
    public bool ValidateNames { get; set; } = false; // OFN_NOVALIDATE

    public bool ShowHidden { get; set; } = false;


    public abstract bool ShowDialog();
}

internal class SaveFileDialog : FileDialog
{
    /// <summary>
    ///  Restores the current directory to its original value if the user
    ///  changed the directory while searching for files.
    /// </summary>
    public bool RestoreDirectory { get; set; } // OFN_NOCHANGEDIR

    /// <summary>
    ///  Gets or sets a value indicating whether the dialog box prompts the user for
    ///  permission to create a file if the user specifies a file that does not exist.
    /// </summary>
    /// <Remarks>
    ///     Callers must have UIPermission.AllWindows to call this API.
    /// </Remarks>
    public bool CreatePrompt { get; set; } = false; // OFN_CREATEPROMPT

    /// <summary>
    /// Gets or sets a value indicating whether the Save As dialog box displays a 
    /// warning if the user specifies a file name that already exists.
    /// </summary>
    /// <Remarks>
    ///     Callers must have UIPermission.AllWindows to call this API.
    /// </Remarks>
    public bool OverwritePrompt { get; set; } = false; // OFN_OVERWRITEPROMPT

    public string? FileName { get; set; }

    public override bool ShowDialog()
    {
        using var fileName = new SafeCoTaskMemString(FileName ?? string.Empty, MAX_FILE_LENGTH);
        using var fileTitle = new SafeCoTaskMemString(MAX_FILE_LENGTH);
        var ofn = new OPENFILENAME
        {
            lStructSize = (uint) Marshal.SizeOf<OPENFILENAME>(),
            lpstrFilter = Filter?.Replace("|", "\0") + "\0",
            nFilterIndex = 1,
            lpstrFileTitle = (IntPtr) fileTitle,
            nMaxFileTitle = (uint) fileTitle.Capacity,
            lpstrInitialDir = string.IsNullOrEmpty(InitialDirectory) ? default : new StrPtrAuto(InitialDirectory),
            lpstrTitle = string.IsNullOrEmpty(Title) ? default : new StrPtrAuto(Title),
            lpstrFile = (IntPtr) fileName,
            nMaxFile = (uint) fileName.Capacity,
            Flags = OFN.OFN_EXPLORER
        };

        if (CheckPathExists)
            ofn.Flags |= OFN.OFN_PATHMUSTEXIST;
        if (!ValidateNames)
            ofn.Flags |= OFN.OFN_NOVALIDATE;
        if (ShowHidden)
            ofn.Flags |= OFN.OFN_FORCESHOWHIDDEN;

        if (RestoreDirectory)
            ofn.Flags |= OFN.OFN_NOCHANGEDIR;
        if (CreatePrompt)
            ofn.Flags |= OFN.OFN_CREATEPROMPT;
        if (OverwritePrompt)
            ofn.Flags |= OFN.OFN_OVERWRITEPROMPT;

        return PInvoke.GetSaveFileName(ref ofn);
    }
}

internal class OpenFileDialog : FileDialog
{
    /// <summary>
    ///  Gets or sets a value indicating whether
    ///  the dialog box displays a warning if the 
    ///  user specifies a file name that does not exist.
    /// </summary>
    public bool CheckFileExists { get; set; } = false; // OFN_FILEMUSTEXIST

    /// <summary>
    /// Gets or sets an option flag indicating whether the 
    /// dialog box allows multiple files to be selected.
    /// </summary>
    public bool Multiselect { get; set; } = false; // OFN_ALLOWMULTISELECT

    /// <summary>
    /// Gets or sets a value indicating whether the read-only 
    /// check box is selected.
    /// </summary>
    public bool ReadOnlyChecked { get; set; } = false; // OFN_READONLY

    /// <summary>
    /// Gets or sets a value indicating whether the dialog 
    /// contains a read-only check box.  
    /// </summary>
    public bool ShowReadOnly { get; set; } = false; // OFN_HIDEREADONLY

    /// <summary>
    ///  Gets or sets a string containing the full path of the file selected in 
    ///  the file dialog box.
    /// </summary>
    public string? FileName => FileNames?.Length > 0 ? FileNames[0] : null;

    /// <summary>
    ///     Gets the file names of all selected files in the dialog box.
    /// </summary>
    public string[]? FileNames { get; protected set; } = null;

    public override bool ShowDialog()
    {
        FileNames = null;

        var file = Marshal.AllocHGlobal(MAX_FILE_LENGTH * Marshal.SystemDefaultCharSize);
        for (var i = 0; i < MAX_FILE_LENGTH * Marshal.SystemDefaultCharSize; i++)
            Marshal.WriteByte(file, i, 0);

        using var fileTitle = string.IsNullOrEmpty(FileName) ? SafeCoTaskMemString.Null : new SafeCoTaskMemString(FileName);
        var ofn = new OPENFILENAME
        {
            lStructSize = (uint) Marshal.SizeOf<OPENFILENAME>(),
            lpstrFilter = Filter?.Replace("|", "\0") + "\0",
            nFilterIndex = 1,
            lpstrFileTitle = (IntPtr) fileTitle,
            nMaxFileTitle = (uint) fileTitle.Capacity,
            lpstrInitialDir = string.IsNullOrEmpty(InitialDirectory) ? default : new StrPtrAuto(InitialDirectory),
            lpstrTitle = string.IsNullOrEmpty(Title) ? default : new StrPtrAuto(Title),
            lpstrFile = file,
            nMaxFile = MAX_FILE_LENGTH,
            Flags = OFN.OFN_EXPLORER
        };

        if (CheckPathExists)
            ofn.Flags |= OFN.OFN_PATHMUSTEXIST;
        if (!ValidateNames)
            ofn.Flags |= OFN.OFN_NOVALIDATE;
        if (ShowHidden)
            ofn.Flags |= OFN.OFN_FORCESHOWHIDDEN;

        if (CheckFileExists)
            ofn.Flags |= OFN.OFN_FILEMUSTEXIST;
        if (Multiselect)
            ofn.Flags |= OFN.OFN_ALLOWMULTISELECT;
        if (ReadOnlyChecked)
            ofn.Flags |= OFN.OFN_READONLY;
        if (!ShowReadOnly)
            ofn.Flags |= OFN.OFN_HIDEREADONLY;

        var result = PInvoke.GetOpenFileName(ref ofn);
        if (result)
        {
            var filePointer = file;
            var pointer = (long) filePointer;
            var fileStr = Marshal.PtrToStringAuto(filePointer);
            var strList = new List<string>();

            // Retrieve file names
            while (fileStr.Length > 0)
            {
                strList.Add(fileStr);

                pointer += fileStr.Length * Marshal.SystemDefaultCharSize + Marshal.SystemDefaultCharSize;
                filePointer = (IntPtr) pointer;
                fileStr = Marshal.PtrToStringAuto(filePointer);
            }

            if (strList.Count > 1)
            {
                FileNames = new string[strList.Count - 1];
                for (var i = 1; i < strList.Count; i++)
                {
                    FileNames[i - 1] = Path.Combine(strList[0], strList[i]);
                }
            }
            else
            {
                FileNames = strList.ToArray();
            }
        }
        return result;
    }
}