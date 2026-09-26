public enum SecrecyMode
{
	Public,
	OnlyKnownToSubject,
	OnlyKnownToObject,
	OnlyKnownToSubjectAndObject,
	OnlyKnownToSubjectCommunity,
	OnlyKnownToObjectCommunity,
	OnlyKnownToSubjectCommunityAndObject,
	OnlyKnownToSubjectCommunityIgnoredByObject,
	OnlyKnownToSubjectCommunityIgnoredByThirdParty,
	OnlyKnownToObjectCommunityIgnoredByThirdParty,
	IgnoredBySubjectCommunity,
	IgnoredByObjectCommunity,
	IgnoredByObject,
	IgnoredByThirdParty,
	Private
}
