#region License

/*-----------------------------------------------------------------------------
Version: 1.0.2.0
FaceBook: https://www.facebook.com/#!/aoonikun
DL Site: http://www48.tok2.com/home/oninonando/
Author: Ao-Oni <ao-oni@mail.goo.ne.jp>
Licensed under The MIT License
Redistributions of files must retain the above copyright notice.
-----------------------------------------------------------------------------*/

#endregion

#region MockEnum

#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace AoOni.MockEnum
{
	#region Default

	public static class MockEnumBaseDefault
	{
		public static string Separator { get { return ", "; } }
	}

	#endregion

	#region Attribute

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class MockEnumFlagsAttribute : Attribute
	{
		public string Separator { get; private set; }

		public MockEnumFlagsAttribute() : this(MockEnumBaseDefault.Separator) { }
		public MockEnumFlagsAttribute(string separator)
		{
			this.Separator = separator;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public sealed class MockEnumMemberAttribute : Attribute
	{
		public object ActualValue { get; private set; }
		public string StringValue { get; private set; }

		public MockEnumMemberAttribute(object actualValue, string stringValue)
		{
			this.ActualValue = actualValue;
			this.StringValue = stringValue;
		}
	}

	#endregion

	#region Base Class

	public class MockEnumBase<ValueType, ClassType> : IComparable<MockEnumBase<ValueType, ClassType>>, IEquatable<MockEnumBase<ValueType, ClassType>>
		where ValueType : struct, IComparable<ValueType>, IEquatable<ValueType>
		where ClassType : MockEnumBase<ValueType, ClassType>, new()
	{
		#region Definition

		private IntPtr _handle;
		private ValueType _actualValue;
		private string _stringValue = null;

		private static bool _isFlag = false;
		private static string _separator = MockEnumBaseDefault.Separator;

		private static Dictionary<IntPtr, ClassType> _instanceList = null;

		#region Calculation Delegate

		private static readonly ParameterExpression param1 = Expression.Parameter(typeof(ValueType));
		private static readonly ParameterExpression param2 = Expression.Parameter(typeof(ValueType));

		private static readonly Func<ValueType, ValueType, ValueType> AndOperate = Expression.Lambda<Func<ValueType, ValueType, ValueType>>
				(
					Expression.MakeBinary(ExpressionType.And, param1, param2),
					param1,
					param2
				).Compile();

		private static readonly Func<ValueType, ValueType, ValueType> OrOperate = Expression.Lambda<Func<ValueType, ValueType, ValueType>>
				(
					Expression.MakeBinary(ExpressionType.Or, param1, param2),
					param1,
					param2
				).Compile();

		private static readonly Func<ValueType, ValueType, ValueType> XorOperate = Expression.Lambda<Func<ValueType, ValueType, ValueType>>
				(
					Expression.MakeBinary(ExpressionType.ExclusiveOr, param1, param2),
					param1,
					param2
				).Compile();

		private static readonly Func<ValueType, ValueType> NotOperate = Expression.Lambda<Func<ValueType, ValueType>>
				(
					Expression.MakeUnary(ExpressionType.Not, param1, typeof(ValueType)),
					param1
				).Compile();

		#endregion

		#endregion

		#region Operator

		#region Cast

		public static implicit operator ClassType(MockEnumBase<ValueType, ClassType> value)
		{
			if (_instanceList.ContainsKey(value._handle))
				return _instanceList[value._handle];

			return new ClassType() { _actualValue = value._actualValue, _stringValue = value._stringValue };
		}

		public static explicit operator ValueType(MockEnumBase<ValueType, ClassType> value)
		{
			return value._actualValue;
		}

		public static explicit operator MockEnumBase<ValueType, ClassType>(ValueType value)
		{
			CreateInstanceList();

			var match = from member in _instanceList.Values where member._actualValue.Equals(value) select member;

			if (match.Count() > 0)
				return match.FirstOrDefault();

			return new ClassType() { _actualValue = value, _stringValue = value.ToString() };
		}

		#endregion

		#region &

		public static MockEnumBase<ValueType, ClassType> operator &(MockEnumBase<ValueType, ClassType> value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return (MockEnumBase<ValueType, ClassType>)AndOperate(value1._actualValue, value2._actualValue);
		}

		#endregion

		#region |

		public static MockEnumBase<ValueType, ClassType> operator |(MockEnumBase<ValueType, ClassType> value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return (MockEnumBase<ValueType, ClassType>)OrOperate(value1._actualValue, value2._actualValue);
		}

		#endregion

		#region ^

		public static MockEnumBase<ValueType, ClassType> operator ^(MockEnumBase<ValueType, ClassType> value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return (MockEnumBase<ValueType, ClassType>)XorOperate(value1._actualValue, value2._actualValue);
		}

		#endregion

		#region ~
		
		public static MockEnumBase<ValueType, ClassType> operator ~(MockEnumBase<ValueType, ClassType> value1)
		{
			return (MockEnumBase<ValueType, ClassType>)NotOperate(value1._actualValue);
		}

		#endregion

		#region ==

		public static bool operator ==(MockEnumBase<ValueType, ClassType> value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return value1.Equals(value2);
		}

		public static bool operator ==(ValueType value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return value1.Equals((ValueType)value2);
		}

		public static bool operator ==(MockEnumBase<ValueType, ClassType> value1, ValueType value2)
		{
			return ((ValueType)value1).Equals(value2);
		}

		#endregion

		#region !=

		public static bool operator !=(MockEnumBase<ValueType, ClassType> value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return !value1.Equals(value2);
		}

		public static bool operator !=(ValueType value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return !value1.Equals((ValueType)value2);
		}

		public static bool operator !=(MockEnumBase<ValueType, ClassType> value1, ValueType value2)
		{
			return !((ValueType)value1).Equals(value2);
		}

		#endregion

		#region <

		public static bool operator <(MockEnumBase<ValueType, ClassType> value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return value1.CompareTo(value2) == -1;
		}

		public static bool operator <(ValueType value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return value1.CompareTo((ValueType)value2) == -1;
		}

		public static bool operator <(MockEnumBase<ValueType, ClassType> value1, ValueType value2)
		{
			return ((ValueType)value1).CompareTo(value2) == -1;
		}

		#endregion

		#region >

		public static bool operator >(MockEnumBase<ValueType, ClassType> value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return value1.CompareTo(value2) == 1;
		}

		public static bool operator >(ValueType value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return value1.CompareTo((ValueType)value2) == 1;
		}

		public static bool operator >(MockEnumBase<ValueType, ClassType> value1, ValueType value2)
		{
			return ((ValueType)value1).CompareTo(value2) == 1;
		}

		#endregion

		#region <=

		public static bool operator <=(MockEnumBase<ValueType, ClassType> value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return value1.CompareTo(value2) <= 0;
		}

		public static bool operator <=(ValueType value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return value1.CompareTo((ValueType)value2) <= 0;
		}

		public static bool operator <=(MockEnumBase<ValueType, ClassType> value1, ValueType value2)
		{
			return ((ValueType)value1).CompareTo(value2) <= 0;
		}

		#endregion

		#region >=

		public static bool operator >=(MockEnumBase<ValueType, ClassType> value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return value1.CompareTo(value2) >= 0;
		}

		public static bool operator >=(ValueType value1, MockEnumBase<ValueType, ClassType> value2)
		{
			return value1.CompareTo((ValueType)value2) >= 0;
		}

		public static bool operator >=(MockEnumBase<ValueType, ClassType> value1, ValueType value2)
		{
			return ((ValueType)value1).CompareTo(value2) >= 0;
		}

		#endregion

		#endregion

		#region Method

		#region Create Instance List

		protected static void CreateInstanceList()
		{
			if (_instanceList != null)
				return;

			_instanceList = new Dictionary<IntPtr, ClassType>();

			var flagAttribute =
						(from classAttribute in typeof(ClassType).GetCustomAttributes(typeof(MockEnumFlagsAttribute), false)
						 select classAttribute).SingleOrDefault() as MockEnumFlagsAttribute;

			if (flagAttribute != null)
			{
				_isFlag = true;
				_separator = flagAttribute.Separator;
			}

			foreach (var fieldInfo in typeof(ClassType).GetFields())
			{
				ClassType tmpEnum;

				var enumAttribute = (from propertyAttribute
										 in fieldInfo.GetCustomAttributes(typeof(MockEnumMemberAttribute), false)
									 select propertyAttribute).SingleOrDefault() as MockEnumMemberAttribute;

				if (enumAttribute == null)
				{
					tmpEnum = new ClassType()
					{
						_handle = fieldInfo.FieldHandle.Value,
						_actualValue = default(ValueType),
						_stringValue = fieldInfo.Name
					};
				}
				else
				{
					tmpEnum = new ClassType()
					{
						_handle = fieldInfo.FieldHandle.Value,
						_actualValue = (ValueType)enumAttribute.ActualValue,
						_stringValue = enumAttribute.StringValue
					};
				}

				_instanceList.Add(tmpEnum._handle, tmpEnum);

				fieldInfo.SetValue(null, tmpEnum);
			}
		}

		#endregion

		#region Equals

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public bool Equals(MockEnumBase<ValueType, ClassType> target)
		{
			return base.Equals(target);
		}

		#endregion

		#region GetHashCode

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

		#region CompareTo

		public int CompareTo(MockEnumBase<ValueType, ClassType> target)
		{
			return _actualValue.CompareTo(target._actualValue);
		}

		#endregion

		#region ToString

		public override string ToString()
		{
			if (_isFlag)
			{
				var valueList = new Dictionary<ValueType, string>();

				foreach (var instance in _instanceList.Values)
				{
					if (instance._actualValue.Equals(_actualValue))
						return instance._stringValue;

					if (instance._actualValue.Equals(default(ValueType)))
						continue;

					if (!AndOperate(_actualValue, instance._actualValue).Equals(instance._actualValue))
						continue;

					if (!valueList.ContainsKey(instance._actualValue))
						valueList.Add(instance._actualValue, instance._stringValue);
				}

				if (valueList.Count > 0)
					return string.Join(_separator, valueList.Values.ToArray());
				else
					return _stringValue;
			}
			else
			{
				if (_stringValue != null)
					return _stringValue;
				else
					return typeof(ClassType).Name;	// Class Name
			}
		}

		#endregion

		#region ToArray

		public static ClassType[] ToArray()
		{
			CreateInstanceList();

			return _instanceList.Values.ToArray();
		}

		#endregion

		#region ToList

		public static List<ClassType> ToList()
		{
			CreateInstanceList();

			return _instanceList.Values.ToList();
		}

		#endregion

		#region TryParse

		public static bool TryParse(string target, out ClassType result)
		{
			CreateInstanceList();

			result = null;

			try
			{
				ClassType tmpResult = null;

				if (_isFlag)
				{
					var targetList = target.Split(new string[] { _separator }, StringSplitOptions.None);

					Array.ForEach(targetList, item =>
					{
						var match = _instanceList.Values.FirstOrDefault(_instance => { return _instance.ToString() == item; }) as ClassType;

						if (match == null)
							throw new Exception("Element Not Found");

						if ((object)tmpResult == null)
							tmpResult = match;
						else
							tmpResult |= match;
					});
				}
				else
				{
					tmpResult = _instanceList.Values.FirstOrDefault(_instance => { return _instance.ToString() == target; }) as ClassType;
				}

				result = tmpResult;

				return result != null;
			}
			catch
			{
				return false;
			}
		}

		#endregion

		#endregion
	}

	#endregion
}

#endregion
