/*
 * CKFinder
 * ========
 * http://ckfinder.com
 * Copyright (C) 2007-2010, CKSource - Frederico Knabben. All rights reserved.
 *
 * The software, this file and its contents are subject to the CKFinder
 * License. Please read the license.txt file before using, installing, copying,
 * modifying or distribute this file or part of its contents. The contents of
 * this file is part of the Source Code of CKFinder.
 *
 */

/**
* @fileOverview
*/

/**
 * Constains the dictionary of language entries.
 * @namespace
 */
CKFinder.lang['sl'] =
{
	appTitle : 'CKFinder', // MISSING

	// Common messages and labels.
	common :
	{
		// Put the voice-only part of the label in the span.
		unavailable		: '%1<span class="cke_accessibility">, unavailable</span>', // MISSING
		confirmCancel	: 'Some of the options have been changed. Are you sure to close the dialog?', // MISSING
		ok				: 'OK', // MISSING
		cancel			: 'Cancel', // MISSING
		confirmationTitle	: 'Confirmation', // MISSING
		messageTitle	: 'Information', // MISSING
		inputTitle		: 'Question', // MISSING
		undo			: 'Undo', // MISSING
		redo			: 'Redo', // MISSING
		skip			: 'Skip', // MISSING
		skipAll			: 'Skip all', // MISSING
		makeDecision	: 'What action should be taken?', // MISSING
		rememberDecision: 'Remember my decision'  // MISSING
	},


	dir : 'ltr', // MISSING
	HelpLang : 'en',
	LangCode : 'sl',

	// Date Format
	//		d    : Day
	//		dd   : Day (padding zero)
	//		m    : Month
	//		mm   : Month (padding zero)
	//		yy   : Year (two digits)
	//		yyyy : Year (four digits)
	//		h    : Hour (12 hour clock)
	//		hh   : Hour (12 hour clock, padding zero)
	//		H    : Hour (24 hour clock)
	//		HH   : Hour (24 hour clock, padding zero)
	//		M    : Minute
	//		MM   : Minute (padding zero)
	//		a    : Firt char of AM/PM
	//		aa   : AM/PM
	DateTime : 'd.m.yyyy H:MM',
	DateAmPm : ['AM', 'PM'],

	// Folders
	FoldersTitle	: 'Mape',
	FolderLoading	: 'Nalagam...',
	FolderNew		: 'Vnesite ime za novo mapo: ',
	FolderRename	: 'Vnesite novo ime za mapo: ',
	FolderDelete	: 'Ali ste prepričani, da želite zbrisati mapo "%1"?',
	FolderRenaming	: ' (Preimenujem...)',
	FolderDeleting	: ' (Brišem...)',

	// Files
	FileRename		: 'Prosimo vnesite novo ime datoteke: ',
	FileRenameExt	: 'Ali ste prepričani, da želite spremeniti končnico datoteke? Možno je, da potem datoteka ne bo uporabna.',
	FileRenaming	: 'Preimenujem...',
	FileDelete		: 'Ali ste prepričani, da želite zbrisati datoteko "%1"?',
	FilesLoading	: 'Loading...', // MISSING
	FilesEmpty		: 'Empty folder', // MISSING
	FilesMoved		: 'File %1 moved into %2:%3', // MISSING
	FilesCopied		: 'File %1 copied into %2:%3', // MISSING

	// Basket
	BasketFolder		: 'Koš',
	BasketClear			: 'Počisti koš',
	BasketRemove		: 'Izbriši iz koša',
	BasketOpenFolder	: 'Open parent folder', // MISSING
	BasketTruncateConfirm : 'Do you really want to remove all files from the basket?', // MISSING
	BasketRemoveConfirm	: 'Do you really want to remove the file "%1" from the basket?', // MISSING
	BasketEmpty			: 'No files in the basket, drag\'n\'drop some.', // MISSING
	BasketCopyFilesHere	: 'Copy Files from Basket', // MISSING
	BasketMoveFilesHere	: 'Move Files from Basket', // MISSING

	BasketPasteErrorOther	: 'File %s error: %e', // MISSING
	BasketPasteMoveSuccess	: 'The following files were moved: %s', // MISSING
	BasketPasteCopySuccess	: 'The following files were copied: %s', // MISSING

	// Toolbar Buttons (some used elsewhere)
	Upload		: 'Naloži',
	UploadTip	: 'Naloži novo datoteko',
	Refresh		: 'Osveži',
	Settings	: 'Nastavitve',
	Help		: 'Pomoč',
	HelpTip		: 'Pomoč',

	// Context Menus
	Select			: 'Izberi',
	SelectThumbnail : 'Izberi malo sličico (predogled)',
	View			: 'Predogled',
	Download		: 'Prenesi na svoj računalnik',

	NewSubFolder	: 'Nova podmapa',
	Rename			: 'Preimenuj',
	Delete			: 'Zbriši',

	CopyDragDrop	: 'Copy file here', // MISSING
	MoveDragDrop	: 'Move file here', // MISSING

	// Dialogs
	RenameDlgTitle		: 'Rename', // MISSING
	NewNameDlgTitle		: 'New name', // MISSING
	FileExistsDlgTitle	: 'File already exists', // MISSING
	SysErrorDlgTitle : 'System error', // MISSING

	FileOverwrite	: 'Overwrite', // MISSING
	FileAutorename	: 'Auto-rename', // MISSING

	// Generic
	OkBtn		: 'OK',
	CancelBtn	: 'Prekliči',
	CloseBtn	: 'Zapri',

	// Upload Panel
	UploadTitle			: 'Naloži novo datoteko',
	UploadSelectLbl		: 'Izberi datoteko za prenos na strežnik',
	UploadProgressLbl	: '(Prenos na strežnik poteka, prosimo počakajte...)',
	UploadBtn			: 'Prenesi izbrano datoteko na strežnik',
	UploadBtnCancel		: 'Cancel', // MISSING

	UploadNoFileMsg		: 'Prosimo izberite datoteko iz svojega računalnika za prenos na strežnik',
	UploadNoFolder		: 'Please select folder before uploading.', // MISSING
	UploadNoPerms		: 'File upload not allowed.', // MISSING
	UploadUnknError		: 'Error sending the file.', // MISSING
	UploadExtIncorrect	: 'File extension not allowed in this folder.', // MISSING

	// Settings Panel
	SetTitle		: 'Nastavitve',
	SetView			: 'Pogled:',
	SetViewThumb	: 'Majhne sličice',
	SetViewList		: 'Seznam',
	SetDisplay		: 'Prikaz:',
	SetDisplayName	: 'Ime datoteke',
	SetDisplayDate	: 'Datum',
	SetDisplaySize	: 'Velikost datoteke',
	SetSort			: 'Sortiranje:',
	SetSortName		: 'po imenu datoteke',
	SetSortDate		: 'po datumu',
	SetSortSize		: 'po velikosti',

	// Status Bar
	FilesCountEmpty : '<Prazna mapa>',
	FilesCountOne	: '1 datoteka',
	FilesCountMany	: '%1 datotek',

	// Size and Speed
	Kb				: '%1 kB',
	KbPerSecond		: '%1 kB/sek',

	// Connector Error Messages.
	ErrorUnknown	: 'Prišlo je do napake. (Napaka %1)',
	Errors :
	{
	 10 : 'Napačni ukaz.',
	 11 : 'V poizvedbi ni bil jasen tip (resource type).',
	 12 : 'Tip datoteke ni primeren.',
	102 : 'Napačno ime mape ali datoteke.',
	103 : 'Vašega ukaza se ne da izvesti zaradi težav z avtorizacijo.',
	104 : 'Vašega ukaza se ne da izvesti zaradi težav z nastavitvami pravic v datotečnem sistemu.',
	105 : 'Napačna končnica datoteke.',
	109 : 'Napačna zahteva.',
	110 : 'Neznana napaka.',
	115 : 'Datoteka ali mapa s tem imenom že obstaja.',
	116 : 'Mapa ni najdena. Prosimo osvežite okno in poskusite znova.',
	117 : 'Datoteka ni najdena. Prosimo osvežite seznam datotek in poskusite znova.',
	118 : 'Source and target paths are equal.', // MISSING
	201 : 'Datoteka z istim imenom že obstaja. Naložena datoteka je bila preimenovana v "%1"',
	202 : 'Neprimerna datoteka.',
	203 : 'Neprimerna datoteka - prevelika je, zasede preveč prostora.',
	204 : 'Naložena datoteka je okvarjena.',
	205 : 'Na strežniku ni na voljo začasna mapa za prenos datotek.',
	206 : 'Nalaganje je bilo prekinjeno zaradi varnostnih razlogov. Datoteka vsebuje podatke, ki spominjajo na HTML kodo.',
	207 : 'Naložena datoteka je bila preimenovana v "%1"',
	300 : 'Moving file(s) failed.', // MISSING
	301 : 'Copying file(s) failed.', // MISSING
	500 : 'Brskalnik je onemogočen zaradi varnostnih razlogov. Prosimo kontaktirajte upravljalca spletnih strani.',
	501 : 'Ni podpore za majhne sličice (predogled).'
	},

	// Other Error Messages.
	ErrorMsg :
	{
		FileEmpty		: 'Ime datoteke ne more biti prazno',
		FileExists		: 'File %s already exists', // MISSING
		FolderEmpty		: 'Mapa ne more biti prazna',

		FileInvChar		: 'Ime datoteke ne sme vsebovati naslednjih znakov: \n\\ / : * ? " < > |',
		FolderInvChar	: 'Ime mape ne sme vsebovati naslednjih znakov: \n\\ / : * ? " < > |',

		PopupBlockView	: 'Datoteke ni možno odpreti v novem oknu. Prosimo nastavite svoj brskalnik tako, da bo dopuščal odpiranje oken (popups) oz. izklopite filtre za blokado odpiranja oken.'
	},

	// Imageresize plugin
	Imageresize :
	{
		dialogTitle		: 'Resize %s', // MISSING
		sizeTooBig		: 'Cannot set image height or width to a value bigger than the original size (%size).', // MISSING
		resizeSuccess	: 'Image resized successfully.', // MISSING
		thumbnailNew	: 'Create new thumbnail', // MISSING
		thumbnailSmall	: 'Small (%s)', // MISSING
		thumbnailMedium	: 'Medium (%s)', // MISSING
		thumbnailLarge	: 'Large (%s)', // MISSING
		newSize			: 'Set new size', // MISSING
		width			: 'Width', // MISSING
		height			: 'Height', // MISSING
		invalidHeight	: 'Invalid height.', // MISSING
		invalidWidth	: 'Invalid width.', // MISSING
		invalidName		: 'Invalid file name.', // MISSING
		newImage		: 'Create new image', // MISSING
		noExtensionChange : 'The file extension cannot be changed.', // MISSING
		imageSmall		: 'Source image is too small',  // MISSING
		contextMenuName	: 'Resize' // MISSING
	},

	// Fileeditor plugin
	Fileeditor :
	{
		save			: 'Save', // MISSING
		fileOpenError	: 'Unable to open file.', // MISSING
		fileSaveSuccess	: 'File saved successfully.', // MISSING
		contextMenuName	: 'Edit', // MISSING
		loadingFile		: 'Loading file, please wait...' // MISSING
	}
};
