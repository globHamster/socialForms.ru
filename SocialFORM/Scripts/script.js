

var docInfo = {
	
	info: {
		title:'Тестовый документ PDF',
		author:'Viktor',
		subject:'Theme',
		keywords:'Ключевые слова'
	},
	
	pageSize:'A4',
	pageOrientation:'landscape',//'portrait'
	pageMargins:[50,50,30,60],
	
	header:function(currentPage,pageCount) {
		return {
			text: currentPage.toString() + 'из' + pageCount,
			alignment:'right',
			margin:[0,30,10,50]
		}
	},
	
	footer:[
		{
			text:'нижний колонтитул',
			alignment:'center',//left  right
		}
	],
	
	content: [
	
		{
			text:'Текст определенного параграфа',
			fontSize:20,
			margin:[150,80, 30,0]
			//pageBreak:'after'
		},
		
		{
			text:'Текст определенного параграфа № 2',
			style:'header'
			//pageBreak:'before'
		},
		
		{
			columns:[
			
				{
					width:'auto',
					text:'Текст колонки №1'
				},
				{
					width:150,
					text:'Текст колонки №2'
				},
				
				{
					width:'*',
					text:'Текст колонки №3'
				},
				
				{
					width:'10%',
					text:'Текст колонки №4'
				},
			
			],
			columnGap: 20
		},
		
		{
			table:{
				widths:['*','auto',150,'*'],
				
				body:[
					['Первая','Вторая','Третья','Четвертая'],
					['Первая','Вторая','Третья','Четвертая'],
					[{text:'текстовое содержимое',bold:true},'Вторая','Третья','Четвертая']
				],
				headerRows:1
			}
		},
		
		{
			ul: [
			
				'Пункт 1',
				'Пункт 2',
				'Пункт 3',
				'Пункт 4',
				{
					text:'Пункт 5',
					bold:true
				}
			
			]
		},
		
		{
			ol: [
			
				'Пункт 1',
				'Пункт 2',
				'Пункт 3',
				'Пункт 4',
				{
					text:'Пункт 5',
					bold:true
				}
			
			]
		}
		
	
	],
	
	styles: {
		header: {
			fontSize:25,
			bold:true,
			alignment:'right'
		}
	}
}

pdfMake.createPdf(docInfo).download('name.pdf');