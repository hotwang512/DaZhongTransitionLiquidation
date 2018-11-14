namespace DaZhongTransitionLiquidation.Infrastructure.UserDefinedEntity
{
    /// <summary>
    /// 接口统一返回类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResultModel<T>
    {
        public T ResultInfo { get; set; }

        public bool IsSuccess { get; set; }

        public string Status { get; set; }
    }

    /// <summary>
    /// 接口统一返回类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class ResultModel<T, T2>
    {
        public T ResultInfo { get; set; }

        public T2 ResultInfo2 { get; set; }

        public bool? IsSuccess { get; set; }

        public string Status { get; set; }
    }

    /// <summary>
    /// 接口统一返回类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    public class ResultModel<T, T2, T3>
    {
        public T ResultInfo { get; set; }

        public T2 ResultInfo2 { get; set; }

        public T3 ResultInfo3 { get; set; }

        public bool? IsSuccess { get; set; }

        public string Status { get; set; }
    }

    /// <summary>
    /// 接口统一返回类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    public class ResultModel<T, T2, T3, T4>
    {
        public T ResultInfo { get; set; }

        public T2 ResultInfo2 { get; set; }

        public T3 ResultInfo3 { get; set; }

        public T4 ResultInfo4 { get; set; }

        public bool? IsSuccess { get; set; }

        public string Status { get; set; }
    }
    /// <summary>
    /// 接口统一返回类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    public class ResultModel<T, T2, T3, T4, T5>
    {
        public T ResultInfo { get; set; }

        public T2 ResultInfo2 { get; set; }

        public T3 ResultInfo3 { get; set; }

        public T4 ResultInfo4 { get; set; }

        public T5 ResultInfo5 { get; set; }

        public bool? IsSuccess { get; set; }

        public string Status { get; set; }
    }
}