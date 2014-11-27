using System.Windows.Shapes;

namespace DrawLineCanvas
{
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