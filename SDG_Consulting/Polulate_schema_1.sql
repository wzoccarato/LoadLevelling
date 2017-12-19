USE [SDG_Consulting]
GO

INSERT INTO [dbo].[Schema]
           ([BlockId],
		   [CubeName],
		   [Heading],
		   [WriteBack])
     VALUES
	 ('a','f1','F1',''),	
	 ('b','f2','F2',''),
	 ('c','f3','F3',''),
	 ('d','CALCEX - IP - Look Ahead Limit (Proc Cat, Ind SS)','CALCEX - IP - Look Ahead Limit (Proc Cat, Ind SS)',''),
	 ('e','CALCEX - IP - Build Late Limit (Prd Cat, Ind SS)','CALCEX - IP - Build Late Limit (Prd Cat, Ind SS)',''),
	 ('f','CALCEX - IP - Priority (Prd Cat, Ind SS)','CALCEX - IP - Priority (Prd Cat, Ind SS)',''),
	 ('g','CALCEX - TCH - Appo Residual Capacity Input','CALCEX - TCH - Appo Residual Capacity Input',''),
	 ('h','CALCEX - TCH - Required Capacity [Ind] Sparse Giuste','CALCEX - TCH - Required Capacity [Ind] Sparse Giuste',''),
	 ('i','CALCEX - TCH - Text Plan BU [Week]','CALCEX - TCH - Text Plan BU [Week]',''),
	 ('j','CALCEX - TCH - Text Plan_HR [Week]','CALCEX - TCH - Text Plan_HR [Week]',''),
	 ('k','CALCEX - TARGET ONTIME','CALCEX - TARGET ONTIME','<-')
GO



