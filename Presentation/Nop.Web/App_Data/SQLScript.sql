IF NOT EXISTS(SELECT * FROM LocaleStringResource WHERE ResourceName = 'Admin.Customers.Customers.Payments')
INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) VALUES (1, 'Admin.Customers.Customers.Payments', 'Payments')
GO

/****** Object:  Table [dbo].[CustomerPayment]    Script Date: 23-Jan-17 2:10:23 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CustomerPayment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedOnUtc] [datetime] NOT NULL,
	[Amount] [decimal] (18,4) NOT NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[Customer_CustomerPayment_Mapping]    Script Date: 23-Jan-17 2:18:36 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Customer_CustomerPayment_Mapping](
	[Customer_Id] [int] NOT NULL,
	[CustomerPayment_Id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Customer_Id] ASC,
	[CustomerPayment_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Customer_CustomerPayment_Mapping]  WITH CHECK ADD  CONSTRAINT [Customer_CustomerPayments_Source] FOREIGN KEY([Customer_Id])
REFERENCES [dbo].[Customer] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Customer_CustomerPayment_Mapping] CHECK CONSTRAINT [Customer_CustomerPayments_Source]
GO

ALTER TABLE [dbo].[Customer_CustomerPayment_Mapping]  WITH CHECK ADD  CONSTRAINT [Customer_CustomerPayment_Target] FOREIGN KEY([CustomerPayment_Id])
REFERENCES [dbo].[CustomerPayment] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Customer_CustomerPayment_Mapping] CHECK CONSTRAINT [Customer_CustomerPayment_Target]
GO
----------------------------------------------------------------------------------------------------------
IF NOT EXISTS(SELECT * FROM LocaleStringResource WHERE ResourceName = 'Admin.Customers.Customers.Accounting')
INSERT INTO LocaleStringResource(LanguageId, ResourceName, ResourceValue) VALUES (1, 'Admin.Customers.Customers.Accounting', 'Accounting')
GO



