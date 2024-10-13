CREATE TABLE [SliceA].[Customer]
(
	[CustomerId] BIGINT NOT NULL IDENTITY, 
    [Name] NVARCHAR(250) NOT NULL, 
    [Address] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_SliceA_Customer] PRIMARY KEY CLUSTERED ([CustomerId] ASC),
)
