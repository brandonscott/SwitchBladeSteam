// This file is automatically generated.
using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Steam4NET
{

	public enum EDenyReason : int
	{
		k_EDenyInvalid = 0,
		k_EDenyInvalidVersion = 1,
		k_EDenyGeneric = 2,
		k_EDenyNotLoggedOn = 3,
		k_EDenyNoLicense = 4,
		k_EDenyCheater = 5,
		k_EDenyLoggedInElseWhere = 6,
		k_EDenyUnknownText = 7,
		k_EDenyIncompatibleAnticheat = 8,
		k_EDenyMemoryCorruption = 9,
		k_EDenyIncompatibleSoftware = 10,
		k_EDenySteamConnectionLost = 11,
		k_EDenySteamConnectionError = 12,
		k_EDenySteamResponseTimedOut = 13,
		k_EDenySteamValidationStalled = 14,
		k_EDenySteamOwnerLeftGuestUser = 15,
	};
	
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	[InteropHelp.CallbackIdentity(201)]
	public struct GSClientApprove_t
	{
		public const int k_iCallback = 201;
		public SteamID_t m_SteamID;
	};
	
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	[InteropHelp.CallbackIdentity(202)]
	public struct GSClientDeny_t
	{
		public const int k_iCallback = 202;
		public SteamID_t m_SteamID;
		public EDenyReason m_eDenyReason;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string m_pchOptionalText;
	};
	
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	[InteropHelp.CallbackIdentity(203)]
	public struct GSClientKick_t
	{
		public const int k_iCallback = 203;
		public SteamID_t m_SteamID;
		public EDenyReason m_eDenyReason;
	};
	
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	[InteropHelp.CallbackIdentity(204)]
	public struct GSClientSteam2Deny_t
	{
		public const int k_iCallback = 204;
		public UInt32 m_UserID;
		public ESteamError m_eSteamError;
	};
	
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	[InteropHelp.CallbackIdentity(205)]
	public struct GSClientSteam2Accept_t
	{
		public const int k_iCallback = 205;
		public UInt32 m_UserID;
		public UInt64 m_SteamID;
	};
	
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	[InteropHelp.CallbackIdentity(206)]
	public struct GSClientAchievementStatus_t
	{
		public const int k_iCallback = 206;
		public SteamID_t m_SteamID;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string m_pchAchievement;
		[MarshalAs(UnmanagedType.I1)]
		public bool m_bUnlocked;
	};
	
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	[InteropHelp.CallbackIdentity(207)]
	public struct GSGameplayStats_t
	{
		public const int k_iCallback = 207;
		public EResult m_eResult;
		public Int32 m_nRank;
		public UInt32 m_unTotalConnects;
		public UInt32 m_unTotalMinutesPlayed;
	};
	
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	[InteropHelp.CallbackIdentity(208)]
	public struct GSClientGroupStatus_t
	{
		public const int k_iCallback = 208;
		public SteamID_t m_SteamIDUser;
		public SteamID_t m_SteamIDGroup;
		[MarshalAs(UnmanagedType.I1)]
		public bool m_bMember;
		[MarshalAs(UnmanagedType.I1)]
		public bool m_bOfficer;
	};
	
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	[InteropHelp.CallbackIdentity(209)]
	public struct GSReputation_t
	{
		public const int k_iCallback = 209;
		public EResult m_eResult;
		public UInt32 m_unReputationScore;
		[MarshalAs(UnmanagedType.I1)]
		public bool m_bBanned;
		public UInt32 m_unBannedIP;
		public UInt16 m_usBannedPort;
		public UInt64 m_ulBannedGameID;
		public UInt32 m_unBanExpires;
	};
	
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	[InteropHelp.CallbackIdentity(210)]
	public struct AssociateWithClanResult_t
	{
		public const int k_iCallback = 210;
		public EResult m_eResult;
	};
	
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	[InteropHelp.CallbackIdentity(211)]
	public struct ComputeNewPlayerCompatibilityResult_t
	{
		public const int k_iCallback = 211;
		public EResult m_eResult;
		public Int32 m_cPlayersThatDontLikeCandidate;
		public Int32 m_cPlayersThatCandidateDoesntLike;
		public Int32 m_cClanPlayersThatDontLikeCandidate;
	};
	
	[StructLayout(LayoutKind.Sequential,Pack=8)]
	[InteropHelp.CallbackIdentity(115)]
	public struct GSPolicyResponse_t
	{
		public const int k_iCallback = 115;
		public Byte m_bSecure;
	};
	
}
