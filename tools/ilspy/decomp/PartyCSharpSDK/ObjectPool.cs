using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace PartyCSharpSDK;

public class ObjectPool
{
	internal class Pool
	{
		internal int limit;

		internal ConstructorInfo ctor;

		internal List<object> objects;

		internal Pool(int limit, ConstructorInfo ctor)
		{
			this.limit = limit;
			this.ctor = ctor;
			objects = new List<object>();
		}
	}

	private static object[] ctorParamList0Element;

	private static object[] ctorParamList1Element;

	private static object[] ctorParamList2Element;

	private Dictionary<Type, Pool> pools;

	static ObjectPool()
	{
		ctorParamList0Element = new object[0];
		ctorParamList1Element = new object[1];
		ctorParamList2Element = new object[2];
	}

	public ObjectPool()
	{
		pools = new Dictionary<Type, Pool>();
	}

	public void AddEntry<T>(int maxLimit, Type[] ctorTypes)
	{
		Type typeFromHandle = typeof(T);
		ConstructorInfo constructor = typeFromHandle.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, ctorTypes, null);
		if (constructor != null)
		{
			pools[typeFromHandle] = new Pool(maxLimit, constructor);
		}
		else
		{
			Debugger.Break();
		}
	}

	public T Retrieve<T>()
	{
		return Retrieve<T>(ctorParamList0Element);
	}

	public T Retrieve<T>(object param)
	{
		ctorParamList1Element[0] = param;
		return Retrieve<T>(ctorParamList1Element);
	}

	public T Retrieve<T>(object param0, object param1)
	{
		ctorParamList2Element[0] = param0;
		ctorParamList2Element[1] = param1;
		return Retrieve<T>(ctorParamList2Element);
	}

	public T Retrieve<T>(object[] ctorParams)
	{
		Type typeFromHandle = typeof(T);
		object obj = null;
		if (pools.ContainsKey(typeFromHandle))
		{
			Pool pool = pools[typeFromHandle];
			ConstructorInfo ctor = pool.ctor;
			int count = pool.objects.Count;
			if (count > 0)
			{
				obj = pool.objects[count - 1];
				pool.objects.RemoveAt(count - 1);
				ctor.Invoke(obj, ctorParams);
			}
			else
			{
				obj = ctor.Invoke(ctorParams);
			}
		}
		else
		{
			Debugger.Break();
		}
		return (T)obj;
	}

	public void Return(object o)
	{
		Type type = o.GetType();
		if (pools.ContainsKey(type) && pools[type].objects.Count < pools[type].limit)
		{
			Pool pool = pools[type];
			if (pool.objects.Count < pool.limit)
			{
				pools[type].objects.Add(o);
			}
		}
	}
}
