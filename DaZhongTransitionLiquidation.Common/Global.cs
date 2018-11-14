using System.Collections.Generic;
using System.Reflection;

namespace DaZhongTransitionLiquidation.Common
{
    public class Global
    {
        /// <summary>
        /// 实体转键值对
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, string> EntityToDictionary<T>(T obj) where T : class
        {
            //初始化定义一个键值对，注意最后的括号
            Dictionary<string, string> dic = new Dictionary<string, string>();
            //返回当前 Type 的所有公共属性Property集合
            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo p in props)
            {
                var property = obj.GetType().GetProperty(p.Name);//获取property对象
                var value = p.GetValue(obj);//获取属性值
                dic.Add(p.Name, value?.ToString() ?? "null");
            }
            return dic;
        }
        public static string Temp
        {
            get { return "/Temp"; }
        }
    }
}