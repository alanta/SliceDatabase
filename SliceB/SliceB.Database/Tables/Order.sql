CREATE TABLE [SliceB].[Order]
(
	[OrderId] BIGINT NOT NULL IDENTITY, 
    [CustomerId] BIGINT NOT NULL, 
    [ContactId] BIGINT NOT NULL,
    [Created] DateTimeOffset NOT NULL, 
     
    CONSTRAINT [PK_SliceB_Order] PRIMARY KEY CLUSTERED ([OrderId] ASC),
    CONSTRAINT [FK_Order_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [SliceA].[Customer]([CustomerId]),
    CONSTRAINT [FK_Order_Contact] FOREIGN KEY ([ContactId]) REFERENCES [SliceA].[Contact]([ContactId])
)
