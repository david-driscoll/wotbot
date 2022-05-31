
CommDKP_DB = {
	["Ashkandi-Horde"] = {
		["Report This"] = {
			["bidpos"] = {
				["y"] = 50.1481590270996,
				["x"] = 271.555572509766,
				["point"] = "LEFT",
				["relativePoint"] = "LEFT",
			},
			["defaults"] = {
				["CommDKPScaleSize"] = 1,
				["HistoryLimit"] = 2500,
				["AutoOpenBid"] = true,
				["DecreaseDisenchantValue"] = false,
				["CustomMaxBid"] = false,
				["CurrentTeam"] = "0",
				["SuppressTells"] = true,
				["BidTimerSize"] = 1,
				["CustomMinBid"] = false,
				["CurrentGuild"] = {
				},
				["SuppressNotifications"] = false,
				["ChatFrames"] = {
					["General"] = true,
					["Combat Log"] = true,
					["Channels"] = true,
				},
				["HideChangeLogs"] = 30204,
				["TooltipHistoryCount"] = 15,
				["DKPHistoryLimit"] = 2500,
			},
			["MaxBidBySlot"] = {
				["Other"] = 200,
				["OneHanded"] = 200,
				["Bracers"] = 200,
				["Neck"] = 200,
				["Belt"] = 200,
				["Hands"] = 200,
				["Boots"] = 200,
				["Ring"] = 200,
				["Cloak"] = 200,
				["Head"] = 200,
				["Trinket"] = 200,
				["Chest"] = 200,
				["OffHand"] = 200,
				["Range"] = 200,
				["TwoHanded"] = 200,
				["Shoulders"] = 200,
				["Legs"] = 200,
			},
			["MinBidBySlot"] = {
				["Other"] = 50,
				["OneHanded"] = 50,
				["Bracers"] = 50,
				["Neck"] = 50,
				["Belt"] = 50,
				["Hands"] = 50,
				["Boots"] = 50,
				["Ring"] = 50,
				["Cloak"] = 50,
				["Head"] = 50,
				["Trinket"] = 50,
				["Chest"] = 50,
				["OffHand"] = 50,
				["Range"] = 50,
				["TwoHanded"] = 50,
				["Shoulders"] = 50,
				["Legs"] = 50,
			},
			["teams"] = {
				["0"] = {
					["name"] = "Report This",
					["index"] = 0,
				},
				["1"] = {
					["name"] = "Report This TEST",
					["index"] = 1,
				},
			},
			["raiders"] = {
			},
			["modes"] = {
				["SameZoneOnly"] = false,
				["ZeroSumBidType"] = "Static",
				["channels"] = {
					["raid"] = true,
					["whisper"] = true,
					["guild"] = true,
				},
				["Inflation"] = 0,
				["rounding"] = 0,
				["ZeroSumBank"] = {
					["balance"] = 0,
				},
				["mode"] = "Bonus Roll",
				["bonus"] = {
					["offspecCost"] = 0,
					["upgradeCost"] = 25,
					["maxDiff"] = 50,
				},
				["StoreBids"] = true,
				["rolls"] = {
					["min"] = 1,
					["AddToMax"] = 0,
					["max"] = 100,
					["UsePerc"] = false,
				},
				["CostSelection"] = "Second Bidder",
				["OnlineOnly"] = false,
				["increment"] = 60,
				["AutoAward"] = true,
				["AntiSnipe"] = 0,
				["SubZeroBidding"] = false,
				["costvalue"] = "Integer",
				["AddToNegative"] = false,
				["AnnounceAward"] = true,
				["AnnounceRaidWarning"] = false,
				["BroadcastOfficerBids"] = true,
				["AllowNegativeBidders"] = false,
				["MaxBehavior"] = "Max DKP",
			},
			["bidintpos"] = {
				["y"] = 81.3641891479492,
				["x"] = -152.345474243164,
				["point"] = "RIGHT",
				["relativePoint"] = "RIGHT",
			},
			["bossargs"] = {
				["CurrentRaidZone"] = "Molten Core",
				["LastKilledBoss"] = "Lucifron",
				["RecentZones"] = {
				},
				["LastKilledNPC"] = {
				},
			},
			["DKPBonus"] = {
				["IncStandby"] = true,
				["IntervalBonus"] = 15,
				["CompletionBonus"] = 10,
				["OnTimeBonus"] = 15,
				["UnexcusedAbsence"] = -25,
				["NewBossKillBonus"] = 10,
				["GiveRaidStart"] = true,
				["BossKillBonus"] = 5,
				["AutoIncStandby"] = true,
				["GiveRaidEnd"] = true,
				["DecayPercentage"] = 20,
				["BidTimer"] = 30,
			},
		},
	},
	["__default"] = {
		["MinBidBySlot"] = {
			["Other"] = 70,
			["OneHanded"] = 70,
			["Bracers"] = 70,
			["Neck"] = 70,
			["Belt"] = 70,
			["Hands"] = 70,
			["Boots"] = 70,
			["Ring"] = 70,
			["Cloak"] = 70,
			["Head"] = 70,
			["Trinket"] = 70,
			["Chest"] = 70,
			["OffHand"] = 70,
			["Range"] = 70,
			["TwoHanded"] = 70,
			["Shoulders"] = 70,
			["Legs"] = 70,
		},
		["raiders"] = {
		},
		["teams"] = {
			["0"] = {
				["name"] = "Report This",
			},
			["1"] = {
				["name"] = "Report This TEST",
			},
		},
		["modes"] = {
			["rolls"] = {
				["min"] = 1,
				["AddToMax"] = 0,
				["max"] = 100,
				["UsePerc"] = false,
			},
			["ZeroSumBidType"] = "Static",
			["channels"] = {
				["raid"] = true,
				["whisper"] = true,
				["guild"] = true,
			},
			["increment"] = 60,
			["AnnounceRaidWarning"] = false,
			["rounding"] = 0,
			["AddToNegative"] = false,
			["AntiSnipe"] = 0,
			["costvalue"] = "Integer",
			["SubZeroBidding"] = false,
			["ZeroSumBank"] = {
				["balance"] = 0,
			},
			["mode"] = "Minimum Bid Values",
			["AllowNegativeBidders"] = false,
			["bonus"] = {
				["offspecCost"] = 0,
				["upgradeCost"] = 25,
				["maxDiff"] = 50,
			},
		},
		["DKPBonus"] = {
			["IncStandby"] = false,
			["CompletionBonus"] = 10,
			["OnTimeBonus"] = 15,
			["UnexcusedAbsence"] = -25,
			["NewBossKillBonus"] = 10,
			["BossKillBonus"] = 5,
			["GiveRaidStart"] = false,
			["DecayPercentage"] = 20,
			["BidTimer"] = 30,
		},
		["bossargs"] = {
			["CurrentRaidZone"] = "Molten Core",
			["LastKilledBoss"] = "Lucifron",
			["RecentZones"] = {
			},
			["LastKilledNPC"] = {
			},
		},
		["MaxBidBySlot"] = {
			["Other"] = 0,
			["OneHanded"] = 0,
			["Bracers"] = 0,
			["Neck"] = 0,
			["Belt"] = 0,
			["Hands"] = 0,
			["Boots"] = 0,
			["Ring"] = 0,
			["Cloak"] = 0,
			["Head"] = 0,
			["Trinket"] = 0,
			["Chest"] = 0,
			["OffHand"] = 0,
			["Range"] = 0,
			["TwoHanded"] = 0,
			["Shoulders"] = 0,
			["Legs"] = 0,
		},
		["defaults"] = {
			["CommDKPScaleSize"] = 1,
			["HistoryLimit"] = 2500,
			["CustomMaxBid"] = true,
			["CurrentTeam"] = "0",
			["SuppressTells"] = true,
			["BidTimerSize"] = 1,
			["CustomMinBid"] = true,
			["CurrentGuild"] = {
			},
			["SuppressNotifications"] = false,
			["ChatFrames"] = {
				["General"] = true,
				["Combat Log"] = true,
				["Channels"] = true,
			},
			["HideChangeLogs"] = 0,
			["TooltipHistoryCount"] = 15,
			["DKPHistoryLimit"] = 2500,
		},
	},
	["dbinfo"] = {
		["priorbuild"] = 0,
		["needsUpgrade"] = false,
		["name"] = "CommDKP_DB",
		["build"] = 30204,
	},
}
CommDKP_Loot = {
	["Ashkandi-Horde"] = {
		["Report This"] = {
			["0"] = {
			},
			["1"] = {
				{
					["zone"] = "Molten Core",
					["boss"] = "Lucifron",
					["loot"] = "|cff0070dd|Hitem:6975::::::::32:::11::::|h[Whirlwind Axe]|h|r",
					["deletes"] = "Sithie-1621056299",
					["date"] = 1621057353,
					["index"] = "Sithie-1621057353",
					["cost"] = 25,
					["player"] = "Sithie",
				}, -- [1]
				{
					["deletedby"] = "Sithie-1621057353",
					["index"] = "Sithie-1621056299",
					["zone"] = "Molten Core",
					["date"] = 1621056299,
					["player"] = "Sithie",
					["loot"] = "|cff0070dd|Hitem:6975::::::::32:::11::::|h[Whirlwind Axe]|h|r",
					["boss"] = "Lucifron",
					["dkp"] = {
						{
							["bidder"] = "Sithie",
							["dkp"] = 25,
						}, -- [1]
					},
					["cost"] = -25,
				}, -- [2]
				["seed"] = 0,
			},
		},
	},
	["__default"] = {
	},
	["dbinfo"] = {
		["priorbuild"] = 0,
		["needsUpgrade"] = false,
		["name"] = "CommDKP_Loot",
		["build"] = 30204,
	},
}
CommDKP_DKPTable = {
	["Ashkandi-Horde"] = {
		["Report This"] = {
			["0"] = {
			},
			["1"] = {
				{
					["previous_dkp"] = 0,
					["dkp"] = 1000,
					["class"] = "WARLOCK",
					["lifetime_gained"] = 1000,
					["player"] = "Felguard",
					["version"] = "Unknown",
					["role"] = "No Role Reported",
					["spec"] = "No Spec Reported",
					["rankName"] = "Officer",
					["lifetime_spent"] = 0,
					["rank"] = 2,
				}, -- [1]
				{
					["previous_dkp"] = 0,
					["dkp"] = 1000,
					["class"] = "SHAMAN",
					["lifetime_gained"] = 1000,
					["player"] = "Mireabella",
					["version"] = "Unknown",
					["role"] = "No Role Reported",
					["spec"] = "No Spec Reported",
					["rankName"] = "Officer",
					["lifetime_spent"] = 0,
					["rank"] = 2,
				}, -- [2]
				{
					["previous_dkp"] = 0,
					["dkp"] = 1000,
					["class"] = "WARRIOR",
					["lifetime_gained"] = 1000,
					["player"] = "Sithie",
					["version"] = "v3.2.4-r62",
					["role"] = "Melee DPS",
					["spec"] = "Arms (23/0/0)",
					["rankName"] = "Officer",
					["lifetime_spent"] = 0,
					["rank"] = 2,
				}, -- [3]
				{
					["previous_dkp"] = 0,
					["dkp"] = 1000,
					["class"] = "PRIEST",
					["lifetime_gained"] = 1000,
					["player"] = "Vansertima",
					["version"] = "Unknown",
					["role"] = "No Role Reported",
					["spec"] = "No Spec Reported",
					["rankName"] = "Officer",
					["lifetime_spent"] = 0,
					["rank"] = 2,
				}, -- [4]
			},
		},
	},
	["__default"] = {
	},
	["dbinfo"] = {
		["priorbuild"] = 0,
		["needsUpgrade"] = false,
		["name"] = "CommDKP_DKPTable",
		["build"] = 30204,
	},
}
CommDKP_DKPHistory = {
	["Ashkandi-Horde"] = {
		["Report This"] = {
			["0"] = {
			},
			["1"] = {
				{
					["players"] = "Sithie,Vansertima,Mireabella,Felguard,",
					["index"] = "Sithie-1621055942",
					["dkp"] = 1000,
					["date"] = 1621055942,
					["reason"] = "Other - test",
				}, -- [1]
             ["2"] = {
                 ["players"] = "Sithie,Vansertima,Mireabella,Felguard,",
                 ["index"] = "Hubs-1621056399",
                 ["dkp"] = "-4,-2,-3,-5,-5%",
                 ["date"] = 1621056399,
                 ["reason"] = "Weekly Decay",
             }, -- [2]
				["seed"] = 1,
			},
		},
	},
	["__default"] = {
	},
	["dbinfo"] = {
		["priorbuild"] = 0,
		["needsUpgrade"] = false,
		["name"] = "CommDKP_DKPHistory",
		["build"] = 30204,
	},
}
CommDKP_MinBids = {
	["Ashkandi-Horde"] = {
		["Report This"] = {
			["0"] = {
			},
			["1"] = {
				["6975"] = {
				},
			},
		},
	},
	["__default"] = {
	},
	["dbinfo"] = {
		["priorbuild"] = 0,
		["needsUpgrade"] = false,
		["name"] = "CommDKP_MinBids",
		["build"] = 30204,
	},
}
CommDKP_MaxBids = {
	["Ashkandi-Horde"] = {
		["Report This"] = {
			["0"] = {
			},
			["1"] = {
			},
		},
	},
	["__default"] = {
	},
	["dbinfo"] = {
		["priorbuild"] = 0,
		["needsUpgrade"] = false,
		["name"] = "CommDKP_MaxBids",
		["build"] = 30204,
	},
}
CommDKP_Whitelist = {
	["Ashkandi-Horde"] = {
		["Report This"] = {
		},
	},
	["__default"] = {
	},
	["dbinfo"] = {
		["priorbuild"] = 0,
		["needsUpgrade"] = false,
		["name"] = "CommDKP_Whitelist",
		["build"] = 30204,
	},
}
CommDKP_Standby = {
	["Ashkandi-Horde"] = {
		["Report This"] = {
			["0"] = {
			},
			["1"] = {
			},
		},
	},
	["__default"] = {
	},
	["dbinfo"] = {
		["priorbuild"] = 0,
		["needsUpgrade"] = false,
		["name"] = "CommDKP_Standby",
		["build"] = 30204,
	},
}
CommDKP_Archive = {
	["Ashkandi-Horde"] = {
		["Report This"] = {
			["0"] = {
			},
			["1"] = {
				["Sithie"] = {
					["deleted"] = true,
				},
			},
		},
	},
	["__default"] = {
	},
	["dbinfo"] = {
		["priorbuild"] = 0,
		["needsUpgrade"] = false,
		["name"] = "CommDKP_Archive",
		["build"] = 30204,
	},
}
CommDKP_Profiles = {
	["Ashkandi-Horde"] = {
		["Report This"] = {
			["1"] = {
				["Annebonelyn"] = {
					["previous_dkp"] = 0,
					["dkp"] = 1000,
					["class"] = "WARLOCK",
					["lifetime_gained"] = 1000,
					["player"] = "Felguard",
					["version"] = "Unknown",
					["role"] = "No Role Reported",
					["spec"] = "No Spec Reported",
					["rankName"] = "Officer",
					["lifetime_spent"] = 0,
					["rank"] = 2,
				},
				["Sithie"] = {
					["previous_dkp"] = 0,
					["dkp"] = 0,
					["class"] = "None",
					["lifetime_gained"] = 0,
					["role"] = "Melee DPS",
					["version"] = "v3.2.4-r62",
					["spec"] = "Arms (23/0/0)",
					["player"] = "Sithie",
					["rankName"] = "None",
					["lifetime_spent"] = 0,
					["rank"] = 20,
				},
			},
			["0"] = {

			}
		},
	},
	["__default"] = {
	},
	["dbinfo"] = {
		["priorbuild"] = 0,
		["needsUpgrade"] = false,
		["name"] = "CommDKP_Profiles",
		["build"] = 30204,
	},
}
