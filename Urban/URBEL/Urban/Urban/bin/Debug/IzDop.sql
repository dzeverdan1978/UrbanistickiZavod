USE [Urban]
GO
ALTER proc [dbo].[DajIzDop] @plan int
as
select id,id_plan,opis,case
when (opis like '%izme%') or (opis like '%prome%') or (opis like '%dopu%') then 'Izmene i dopune'
else 'Uticaji'
end as Vrsta 
from IzDop where id_plan=@plan

