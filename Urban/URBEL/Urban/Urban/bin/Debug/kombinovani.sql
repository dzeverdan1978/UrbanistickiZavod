use urban
go
create index ix_kartbroj
on regupr (kartbroj)
go

ALTER proc [dbo].[DajIzDop] @plan int,@kombinovani bit
as
if @kombinovani=0
begin
select id,id_plan,opis,case
when (opis like '%izmene i dopune%') or (opis like '%izmena i dopuna%') or (opis like '%izmena%') or (opis like '%izmene%') then 'Izmene i dopune'
else 'Uticaji'
end as Vrsta 
from IzDop where id_plan=@plan
end
else
begin
declare @kart varchar(50)
select @kart=kartbroj from regupr where id=@plan
if @kart is not null
begin
select id,@plan,' '+kartbroj+'  '+nazprp,'Izmene i dopune' as Vrste
from Regupr
where kartbroj like @kart+'-%'
end
end
go


