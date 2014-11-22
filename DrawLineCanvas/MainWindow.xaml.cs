// Last Change: 2014 11 13 7:45 PM

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;


namespace DrawLineCanvas
{
	/// <summary>
	///     Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		/// <summary>
		///     Мы рисуем первую линию
		/// </summary>
		private bool _firstLine = true;

		/// <summary>
		///     В настоящий момент мы рисуем линию, а не просто елозим мышкой
		/// </summary>
		private bool _drawingMode;

		/// <summary>
		///     Временная линия
		/// </summary>
		private Line _tempLine;

		/// <summary>
		///     Рисуем вертикальную линию
		/// </summary>
		private bool _isVertical = true;

		/// <summary>
		///     Предыдущая координата X
		/// </summary>
		private double _prevX;

		/// <summary>
		///     Предыдущая координата Y
		/// </summary>
		private double _prevY;

		/// <summary>
		///     Новая координата X
		/// </summary>
		private double _newX;

		/// <summary>
		///     Новая координата Y
		/// </summary>
		private double _newY;

		/// <summary>
		///     Список всех нарисованных линий
		/// </summary>
		private readonly List<LineInfo> _linesList = new List<LineInfo>();

		/// <summary>
		/// Время открытия файла
		/// </summary>
		private DateTime _time;

		/// <summary>
		/// Выделение области определения функции
		/// </summary>
		private bool _drawRectangle;

		/// <summary>
		/// Начальная точка для построения квадрата по оси X
		/// </summary>
		private double begin_x;

		/// <summary>
		/// Начальная точка для построения квадрата по оси Y
		/// </summary>
		private double begin_y;

		public MainWindow()
		{
			InitializeComponent();
			_drawRectangle = false;
		}

		/// <summary>
		/// Канвас подгоняем под размер изображения
		/// </summary>
		private void ResizeImage()
		{
			CnvDraw.Height = ImgWell.Source.Height;
			CnvDraw.Width = ImgWell.Source.Width;
		}

		/// <summary>
		///     Повели мышкой по канвасу
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_MouseMove(object sender, MouseEventArgs e)
		{
			//режим задания области определения
			if (_drawRectangle && e.LeftButton == MouseButtonState.Pressed)
			{
				
				begin_x = e.GetPosition(CnvDraw).X;
				begin_y = e.GetPosition(CnvDraw).Y;
				var rectangle = new Rectangle {Stroke = Brushes.LightBlue, Width = 400, Height = 400};
				//Canvas.SetLeft(rectangle, 0);
				//Canvas.SetTop(rectangle, 0);
				CnvDraw.Children.Add(rectangle);
				//переопределяем канвас под область определения
				CnvDraw.Height = rectangle.Height;
				CnvDraw.Width = rectangle.Width;
				_drawRectangle = false;
			}
			else
			{
				// если левая кнопка нажата, рисуем временную линию
				if (e.LeftButton == MouseButtonState.Pressed && (DateTime.Now - _time).TotalMilliseconds > 100)
				{
					// говорим флагу что мы начали рисовать линию
					_drawingMode = true;
					CnvDraw.Children.Remove(_tempLine);
					// мы рисуем первую линию
					if (_firstLine)
					{
						_prevX = e.GetPosition(CnvDraw).X;
						_prevY = 0;
						_newX = _prevX;
						_newY = e.GetPosition(CnvDraw).Y;
					}
					// мы рисуем не первую линию
					else
					{
						// рисуем вертикальную линию
						if (_isVertical)
						{
							_newX = _prevX;
							_newY = e.GetPosition(CnvDraw).Y;
						}
						// рисуем горизонтальную линию
						else
						{
							_newX = e.GetPosition(CnvDraw).X;
							_newY = _prevY;
						}
					}
					_tempLine = new Line
					{
						X1 = _prevX,
						Y1 = _prevY,
						X2 = _newX,
						Y2 = _newY,
						Stroke = Brushes.Red
					};
					CnvDraw.Children.Add(_tempLine);
				}
				// если левая кнопка не нажата, и мы были в режиме рисовашек
				else if (_drawingMode)
				{
					_drawingMode = false;
					// Йухххууу! Мы нарисовали первую линию епта!
					if (_firstLine)
					{
						_firstLine = false;
					}

					// создаем новую линию взамен временной
					var line = new Line
					{
						X1 = _prevX,
						Y1 = _prevY,
						X2 = _newX,
						Y2 = _newY,
						Stroke = Brushes.Black
					};
					CnvDraw.Children.Add(line);
					CnvDraw.Children.Remove(_tempLine);
					_linesList.Add(new LineInfo(_prevX, _prevY, line));
					//определение ребаных координат линии
					X.Content = Math.Abs(line.X1 - line.X2);
					Y.Content = Math.Abs(line.Y1 - line.Y2);

					// запоминаем координаты
					_prevX = _newX;
					_prevY = _newY;

					// переключаем режим рисования на обратный
					_isVertical = !_isVertical;
				}
			}
		}

		/// <summary>
		///     Удаляет последнюю линию по backspace
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Back && !_drawingMode && _linesList.Count > 0)
			{
				int lastIndex = _linesList.Count - 1;
				if (_linesList.Count >= 2)
				{
					_prevX = _linesList[lastIndex].StartX;
					_prevY = _linesList[lastIndex].StartY;
				}
				else
				{
					_prevY = 0;
					_firstLine = true;
				}
				CnvDraw.Children.Remove(_linesList[lastIndex].LineReference);
				_linesList.RemoveAt(_linesList.Count - 1);
				_isVertical = !_isVertical;
			}
		}
		/// <summary>
		/// Загрузка изображения
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButLoad_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var dlg = new OpenFileDialog
			{
				FileName = " Image ",
				DefaultExt = ".jpg",
				Filter = " Image (.jpg)|*.jpg"
			};
			bool? result = dlg.ShowDialog();
			if (result == true)
			{
				string filename = dlg.FileName;
				var bi3 = new BitmapImage();
				bi3.BeginInit();
				bi3.UriSource = new Uri(filename, UriKind.Absolute);
				bi3.EndInit();
				ImgWell.Source = bi3;
				_time = DateTime.Now;
				
			}
			ResizeImage();
		}

		/// <summary>
		/// Задание области определения функции
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButCoord_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_drawRectangle = true;
		}
	}

	/// <summary>
	///     Информация о линии
	/// </summary>
	internal class LineInfo
	{
		/// <summary>
		///     X координата первой точки
		/// </summary>
		public readonly double StartX;

		/// <summary>
		///     Y координата первой точки
		/// </summary>
		public readonly double StartY;

		/// <summary>
		///     Ссылка на линию
		/// </summary>
		public readonly Line LineReference;

		/// <summary>
		///     Конструктор для создания новой записи о лини
		/// </summary>
		/// <param name="startX">X координата первой точки</param>
		/// <param name="startY">Y координата первой точки</param>
		/// <param name="lineReference">Ссылка на линию</param>
		public LineInfo(double startX, double startY, Line lineReference)
		{
			StartX = startX;
			StartY = startY;
			LineReference = lineReference;
		}
	}
}