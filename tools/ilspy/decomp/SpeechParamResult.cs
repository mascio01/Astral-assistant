public struct SpeechParamResult : IReflectable
{
	public int Hash;

	public BaseObject Obj;

	public EquipmentPrototype EquipmentProto;

	public LiquidPrototype Liquid;

	public PropPrototype PropProto;

	public string Str1;

	public string Str2;

	public SpeechParamModifier Modifier;

	public float NumericVal;

	public static SpeechParamResult Create(BaseObject obj)
	{
		return new SpeechParamResult
		{
			Obj = obj
		};
	}

	public static SpeechParamResult Create(BaseObject obj, int hash)
	{
		return new SpeechParamResult
		{
			Obj = obj,
			Hash = hash
		};
	}

	public static SpeechParamResult Create(EquipmentPrototype proto, float num = 0f)
	{
		return new SpeechParamResult
		{
			EquipmentProto = proto,
			NumericVal = num
		};
	}

	public static SpeechParamResult Create(LiquidPrototype liquid)
	{
		return new SpeechParamResult
		{
			Liquid = liquid
		};
	}

	public static SpeechParamResult Create(PropPrototype proto)
	{
		return new SpeechParamResult
		{
			PropProto = proto
		};
	}

	public static SpeechParamResult Create(float num)
	{
		return new SpeechParamResult
		{
			NumericVal = num
		};
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Hash);
		reflector.Add(ref Obj);
		reflector.AddAfter(ref EquipmentProto, 588);
		reflector.AddAfter(ref Liquid, 611);
		reflector.AddAfter(ref PropProto, 610);
		reflector.Add(ref Str1);
		reflector.AddAfter(ref Str2, 210);
		reflector.Add(ref Modifier);
		reflector.AddAfter(ref NumericVal, 396);
	}

	public GenderType GetGender(Language language)
	{
		if (Obj != null)
		{
			return Obj.GetGender(language);
		}
		if (Liquid != null)
		{
			return Liquid.GetGenderInLanguage(language);
		}
		if (EquipmentProto != null)
		{
			return EquipmentProto.GetGenderInLanguage(language);
		}
		if (PropProto != null)
		{
			return PropProto.GetGenderInLanguage(language);
		}
		return GenderType.Count;
	}
}
