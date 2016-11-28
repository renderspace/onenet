SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO

USE [sistem2] 
GO

EXEC sp_addrole 'One.Net.FrontEnd'

GRANT SELECT TO [One.Net.FrontEnd]


GRANT EXECUTE ON GetPageFromURI TO [One.Net.FrontEnd]  
GRANT EXECUTE ON DeleteNewsletterSubscription TO [One.Net.FrontEnd]  
GRANT EXECUTE ON ListPagedRssFeeds TO [One.Net.FrontEnd]  
GRANT EXECUTE ON Acc_Str2Number TO [One.Net.FrontEnd]  
GRANT EXECUTE ON Acc_IntList2Table TO [One.Net.FrontEnd]  
GRANT EXECUTE ON Acc_CharList2Table TO [One.Net.FrontEnd]  
GRANT EXECUTE ON email_modify TO [One.Net.FrontEnd]  
GRANT EXECUTE ON newsletter_subscription_get TO [One.Net.FrontEnd]  
GRANT EXECUTE ON GetMenu_acc_Path TO [One.Net.FrontEnd]  
GRANT EXECUTE ON GetMenu_acc_Hierarchy TO [One.Net.FrontEnd]  
GRANT EXECUTE ON GetMenu TO [One.Net.FrontEnd]  
GRANT EXECUTE ON GetPath TO [One.Net.FrontEnd]  

GRANT UPDATE,INSERT ON [dbo].[newsletter_subscription] TO [One.Net.FrontEnd]
GRANT UPDATE,INSERT ON [dbo].[files] TO [One.Net.FrontEnd]

-- for file upload on frontend
-- GRANT INSERT ON [dbo].[content_data_store_audit] TO [One.Net.FrontEnd]
-- GRANT DELETE ON [dbo].[ucategorie_belongs_to] TO [One.Net.FrontEnd]
-- GRANT EXECUTE ON [dbo].[ChangeUCategorieBelongsTo] TO [One.Net.FrontEnd]

EXEC sp_addrole 'One.Net.BackEnd'

GRANT SELECT TO [One.Net.BackEnd]

GRANT EXECUTE ON GetPageFromURI TO [One.Net.BackEnd]  
GRANT EXECUTE ON ChangePage TO [One.Net.BackEnd]  
GRANT EXECUTE ON ChangePageSetting TO [One.Net.BackEnd]  
GRANT EXECUTE ON ChangeModuleInstance TO [One.Net.BackEnd]  
GRANT EXECUTE ON ChangeModuleInstanceSetting TO [One.Net.BackEnd]  
GRANT EXECUTE ON ChangeContent TO [One.Net.BackEnd]  
GRANT EXECUTE ON ChangeIntLink TO [One.Net.BackEnd]  
GRANT EXECUTE ON ListPagedNewsletterSubscriptions TO [One.Net.BackEnd]  
GRANT EXECUTE ON DeleteNewsletterSubscription TO [One.Net.BackEnd]  
GRANT EXECUTE ON ListPagedRssFeeds TO [One.Net.BackEnd]  
GRANT EXECUTE ON ChangeUCategorieBelongsTo TO [One.Net.BackEnd]  
GRANT EXECUTE ON SwapOrderOfCategorizedItems TO [One.Net.BackEnd]  
GRANT EXECUTE ON nextval TO [One.Net.BackEnd]  
GRANT EXECUTE ON Acc_Str2Number TO [One.Net.BackEnd]  
GRANT EXECUTE ON Acc_IntList2Table TO [One.Net.BackEnd]  
GRANT EXECUTE ON Acc_CharList2Table TO [One.Net.BackEnd]  
GRANT EXECUTE ON ChangeEvent TO [One.Net.BackEnd]  
GRANT EXECUTE ON email_modify TO [One.Net.BackEnd]  
GRANT EXECUTE ON newsletter_subscription_get TO [One.Net.BackEnd]  

GRANT EXECUTE ON GetMenu_acc_Path TO [One.Net.BackEnd]  
GRANT EXECUTE ON GetMenu_acc_Hierarchy TO [One.Net.BackEnd]  
GRANT EXECUTE ON GetMenu TO [One.Net.BackEnd]  
GRANT EXECUTE ON GetPath TO [One.Net.BackEnd]  
GRANT EXECUTE ON DeletePage TO [One.Net.BackEnd]  
GRANT EXECUTE ON ChangeWebSiteSetting TO [One.Net.BackEnd] 

GRANT UPDATE TO [One.Net.BackEnd]
GRANT DELETE TO [One.Net.BackEnd]
GRANT INSERT TO [One.Net.BackEnd]


