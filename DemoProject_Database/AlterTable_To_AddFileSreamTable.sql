ALTER DATABASE DemoProject ADD FILEGROUP FILESTREAM_grp CONTAINS FILESTREAM
GO

ALTER DATABASE DemoProject ADD FILE ( NAME = N'DemoSQLFiles', FILENAME = N'C:\SQLFileStream\FS' ) TO FILEGROUP FILESTREAM_grp
GO



/* rename the varbinary(max) column
eg. FileData to xxFileData */
sp_RENAME 'MenuDetails.Image', 'xxImage' , 'COLUMN'
GO

/*set not null UUID need for filestream */
Alter table MenuDetails
Add UUID uniqueidentifier not null ROWGUIDCOL unique default newid()
GO

/* create a new varbinary(max) FILESTREAM column */
ALTER TABLE MenuDetails
ADD Image varbinary(max) FILESTREAM NULL
GO

/* move the contents of varbinary(max) column to varbinary(max) FILESTREAM column */
UPDATE MenuDetails
SET Image = xxImage
GO

/* drop the xx<ColumnName> column */
ALTER TABLE MenuDetails
DROP COLUMN xxImage
GO