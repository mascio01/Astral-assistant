public struct IconGenerationRequest
{
	public IconGenerationRequestType Type;

	public object Object;

	public BaseObjectType BaseObjectType;

	public int LastRequestedFrame;

	public static IconGenerationRequest Face(Character character)
	{
		IconGenerationRequest result = default(IconGenerationRequest);
		result.Type = IconGenerationRequestType.Face;
		result.Object = character;
		result.LastRequestedFrame = GameImpl.FrameCount;
		result.BaseObjectType = BaseObjectType.Invalid;
		return result;
	}

	public static IconGenerationRequest Full(Character character)
	{
		IconGenerationRequest result = default(IconGenerationRequest);
		result.Type = IconGenerationRequestType.Full;
		result.Object = character;
		result.LastRequestedFrame = GameImpl.FrameCount;
		result.BaseObjectType = BaseObjectType.Invalid;
		return result;
	}

	public static IconGenerationRequest Clothing(Equipment item)
	{
		IconGenerationRequest result = default(IconGenerationRequest);
		result.Type = IconGenerationRequestType.Clothing;
		result.Object = item;
		result.LastRequestedFrame = GameImpl.FrameCount;
		result.BaseObjectType = BaseObjectType.Invalid;
		return result;
	}

	public static IconGenerationRequest Prop(Prop prop)
	{
		IconGenerationRequest result = default(IconGenerationRequest);
		result.Type = IconGenerationRequestType.Prop;
		result.Object = prop;
		result.LastRequestedFrame = GameImpl.FrameCount;
		result.BaseObjectType = BaseObjectType.Invalid;
		return result;
	}

	public static IconGenerationRequest PropPrototype(PropPrototype proto)
	{
		IconGenerationRequest result = default(IconGenerationRequest);
		result.Type = IconGenerationRequestType.PropPrototype;
		result.Object = proto;
		result.LastRequestedFrame = GameImpl.FrameCount;
		result.BaseObjectType = BaseObjectType.Invalid;
		return result;
	}

	public static IconGenerationRequest ObjectType(BaseObjectType type)
	{
		IconGenerationRequest result = default(IconGenerationRequest);
		result.Type = IconGenerationRequestType.BaseObjectType;
		result.BaseObjectType = type;
		result.LastRequestedFrame = GameImpl.FrameCount;
		result.Object = null;
		return result;
	}

	public bool IsNull()
	{
		return Type == IconGenerationRequestType.None;
	}

	public bool Equals(IconGenerationRequest req)
	{
		if (req.Type == Type && req.Object == Object)
		{
			return req.BaseObjectType == BaseObjectType;
		}
		return false;
	}
}
