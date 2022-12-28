insert into UserRegistration 
values ('Jeet','w`mns`os','jeetptl@jin.com',8716237489,GETDATE())

ALTER TABLE UserRegistration ADD CONSTRAINT DF_UserRegistration DEFAULT GETDATE() FOR Date

select * from MealType

insert into MealType values
('MainCourse'),--2
('Side Dish'),--3
('Breakfast'),--4
('Appetizer'),--5
('Soups'),--6
('Salads'),--7
('Desserts'),--8
('Beverages'),--9
('Baking')--10

update MenuDetails 
set Name = 'Hungarian Mushroom Soup'
where ItemID = 1

select * from MenuDetails

delete from MenuDetails where ItemID = 1016

select * from MenuItemArchives

insert into MenuDetails values
('Pumpkin Curry Soup',(SELECT * FROM OPENROWSET(BULK N'C:\Users\meet.dadhania\Pictures\Saved Pictures\Pumpkin-Curry-Soup.jpg', SINGLE_BLOB) as T1),6,150,1,GETDATE()),
('Tomato Basil Bisque',(SELECT * FROM OPENROWSET(BULK N'C:\Users\meet.dadhania\Pictures\Saved Pictures\Tomato-Basil-Bisque.jpg', SINGLE_BLOB) as T1),6,130,1,GETDATE())

select * from UserRegistration
where UserName = 'Mahiraj'

delete from UserRegistration
where UserName = 'mahiraj'



delete from UserRegistration 
where UserID <=7

create TRIGGER MenuItem_BeforeDelete
    ON MenuDetails
    for DELETE
AS
  BEGIN
    INSERT INTO MenuItemArchives(ItemID, Name,Image,TypeID,Price,Veg,CreatedBy,CreatedOn,DeletedOn)
       SELECT 
            d.ItemID,d.Name,d.Image,d.TypeID,d.Price,d.Veg,d.CreatedBy,d.CreatedOn,GETDATE()
       FROM deleted d 
  END
GO

