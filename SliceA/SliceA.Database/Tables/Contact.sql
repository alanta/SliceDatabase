CREATE TABLE [SliceA].[Contact]
(
	[ContactId] BIGINT NOT NULL IDENTITY, 
    [FirstName] NVARCHAR(250) NOT NULL, 
    [LastName] NVARCHAR(250) NOT NULL, 
    [CustomerId] BIGINT NOT NULL, 
    CONSTRAINT [PK_SliceA_Contact] PRIMARY KEY CLUSTERED ([ContactId] ASC),
    CONSTRAINT [FK_Contact_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [SliceA].[Customer]([CustomerId])
)
GO
CREATE INDEX IX_Contact_CustomerId ON [SliceA].[Contact] (CustomerId);
