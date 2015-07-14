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
 * @fileOverview Defines the {@link CKFinder.lang} object, for the Danish
 *		language. This is the base file for all translations.
*/

/**
 * Constains the dictionary of language entries.
 * @namespace
 */
CKFinder.lang['da'] =
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
	LangCode : 'da',

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
	DateTime : 'dd/mm/yyyy HH:MM',
	DateAmPm : ['AM', 'PM'],

	// Folders
	FoldersTitle	: 'Mapper',
	FolderLoading	: 'Indlæser...',
	FolderNew		: 'Skriv navnet på den nye mappe: ',
	FolderRename	: 'Skriv det nye navn på mappen: ',
	FolderDelete	: 'Er du sikker på, at du vil slette mappen "%1" ?',
	FolderRenaming	: ' (Omdøber...)',
	FolderDeleting	: ' (Sletter...)',

	// Files
	FileRename		: 'Skriv navnet på den nye fil: ',
	FileRenameExt	: 'Er du sikker på, at du vil ændre filtypen? Filen kan muligvis ikke bruges bagefter.',
	FileRenaming	: '(Omdøber...)',
	FileDelete		: 'Er du sikker på, at du vil slette filen "%1" ?',
	FilesLoading	: 'Loading...', // MISSING
	FilesEmpty		: 'Empty folder', // MISSING
	FilesMoved		: 'File %1 moved into %2:%3', // MISSING
	FilesCopied		: 'File %1 copied into %2:%3', // MISSING

	// Basket
	BasketFolder		: 'Basket', // MISSING
	BasketClear			: 'Clear Basket', // MISSING
	BasketRemove		: 'Remove from basket', // MISSING
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
	Upload		: 'Upload',
	UploadTip	: 'Upload ny fil',
	Refresh		: 'Opdatér',
	Settings	: 'Indstillinger',
	Help		: 'Hjælp',
	HelpTip		: 'Hjælp',

	// Context Menus
	Select			: 'Vælg',
	SelectThumbnail : 'Vælg thumbnail',
	View			: 'Vis',
	Download		: 'Download',

	NewSubFolder	: 'Ny undermappe',
	Rename			: 'Omdøb',
	Delete			: 'Slet',

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
	CancelBtn	: 'Annullér',
	CloseBtn	: 'Luk',

	// Upload Panel
	UploadTitle			: 'Upload ny fil',
	UploadSelectLbl		: 'Vælg den fil, som du vil uploade',
	UploadProgressLbl	: '(Uploader, vent venligst...)',
	UploadBtn			: 'Upload valgt fil',
	UploadBtnCancel		: 'Cancel', // MISSING

	UploadNoFileMsg		: 'Vælg en fil på din computer',
	UploadNoFolder		: 'Please select folder before uploading.', // MISSING
	UploadNoPerms		: 'File upload not allowed.', // MISSING
	UploadUnknError		: 'Error sending the file.', // MISSING
	UploadExtIncorrect	: 'File extension not allowed in this folder.', // MISSING

	// Settings Panel
	SetTitle		: 'Indstillinger',
	SetView			: 'Vis:',
	SetViewThumb	: 'Thumbnails',
	SetViewList		: 'Liste',
	SetDisplay		: 'Thumbnails:',
	SetDisplayName	: 'Filnavn',
	SetDisplayDate	: 'Dato',
	SetDisplaySize	: 'Størrelse',
	SetSort			: 'Sortering:',
	SetSortName		: 'efter filnavn',
	SetSortDate		: 'efter dato',
	SetSortSize		: 'efter størrelse',

	// Status Bar
	FilesCountEmpty : '<tom mappe>',
	FilesCountOne	: '1 fil',
	FilesCountMany	: '%1 filer',

	// Size and Speed
	Kb				: '%1 kB',
	KbPerSecond		: '%1 kB/s',

	// Connector Error Messages.
	ErrorUnknown	: 'Det var ikke muligt at fuldføre handlingen. (Fejl: %1)',
	Errors :
	{
	 10 : 'Ugyldig handling.',
	 11 : 'Ressourcetypen blev ikke angivet i anmodningen.',
	 12 : 'Ressourcetypen er ikke gyldig.',
	102 : 'Ugyldig fil eller mappenavn.',
	103 : 'Det var ikke muligt at fuldføre handlingen på grund af en begrænsning i rettigheder.',
	104 : 'Det var ikke muligt at fuldføre handlingen på grund af en begrænsning i filsystem rettigheder.',
	105 : 'Ugyldig filtype.',
	109 : 'Ugyldig anmodning.',
	110 : 'Ukendt fejl.',
	115 : 'En fil eller mappe med det samme navn eksisterer allerede.',
	116 : 'Mappen blev ikke fundet. Opdatér listen eller prøv igen.',
	117 : 'Filen blev ikke fundet. Opdatér listen eller prøv igen.',
	118 : 'Source and target paths are equal.', // MISSING
	201 : 'En fil med det samme filnavn eksisterer allerede. Den uploadede fil er blevet omdøbt til "%1"',
	202 : 'Ugyldig fil.',
	203 : 'Ugyldig fil. Filstørrelsen er for stor.',
	204 : 'Den uploadede fil er korrupt.',
	205 : 'Der er ikke en midlertidig mappe til upload til rådighed på serveren.',
	206 : 'Upload annulleret af sikkerhedsmæssige årsager. Filen indeholder HTML-lignende data.',
	207 : 'Den uploadede fil er blevet omdøbt til "%1"',
	300 : 'Moving file(s) failed.', // MISSING
	301 : 'Copying file(s) failed.', // MISSING
	500 : 'Filbrowseren er deaktiveret af sikkerhedsmæssige årsager. Kontakt systemadministratoren eller kontrollér CKFinders konfigurationsfil.',
	501 : 'Understøttelse af thumbnails er deaktiveret.'
	},

	// Other Error Messages.
	ErrorMsg :
	{
		FileEmpty		: 'Filnavnet må ikke være tomt',
		FileExists		: 'File %s already exists', // MISSING
		FolderEmpty		: 'Mappenavnet må ikke være tomt',

		FileInvChar		: 'Filnavnet må ikke indeholde et af følgende tegn: \n\\ / : * ? " < > |',
		FolderInvChar	: 'Mappenavnet må ikke indeholde et af følgende tegn: \n\\ / : * ? " < > |',

		PopupBlockView	: 'Det var ikke muligt at åbne filen i et nyt vindue. Kontrollér konfigurationen i din browser, og deaktivér eventuelle popup-blokkere for denne hjemmeside.'
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
