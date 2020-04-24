using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Verse
{
	public class RaceProperties
	{
		public Intelligence intelligence;

		private FleshTypeDef fleshType;

		private ThingDef bloodDef;

		public bool hasGenders = true;

		public bool needsRest = true;

		public ThinkTreeDef thinkTreeMain;

		public ThinkTreeDef thinkTreeConstant;

		public PawnNameCategory nameCategory;

		public FoodTypeFlags foodType;

		public BodyDef body;

		public Type deathActionWorkerClass;

		public List<AnimalBiomeRecord> wildBiomes;

		public SimpleCurve ageGenerationCurve;

		public bool makesFootprints;

		public int executionRange = 2;

		public float lifeExpectancy = 10f;

		public List<HediffGiverSetDef> hediffGiverSets;

		public bool herdAnimal;

		public bool packAnimal;

		public bool predator;

		public float maxPreyBodySize = 99999f;

		public float wildness;

		public float petness;

		public float nuzzleMtbHours = -1f;

		public float manhunterOnDamageChance;

		public float manhunterOnTameFailChance;

		public bool canBePredatorPrey = true;

		public bool herdMigrationAllowed = true;

		public float gestationPeriodDays = 10f;

		public SimpleCurve litterSizeCurve;

		public float mateMtbHours = 12f;

		[NoTranslate]
		public List<string> untrainableTags;

		[NoTranslate]
		public List<string> trainableTags;

		public TrainabilityDef trainability;

		private RulePackDef nameGenerator;

		private RulePackDef nameGeneratorFemale;

		public float nameOnTameChance;

		public float nameOnNuzzleChance;

		public float baseBodySize = 1f;

		public float baseHealthScale = 1f;

		public float baseHungerRate = 1f;

		public List<LifeStageAge> lifeStageAges = new List<LifeStageAge>();

		[MustTranslate]
		public string meatLabel;

		public Color meatColor = Color.white;

		public float meatMarketValue = 2f;

		public ThingDef useMeatFrom;

		public ThingDef useLeatherFrom;

		public ThingDef leatherDef;

		public ShadowData specialShadowData;

		public IntRange soundCallIntervalRange = new IntRange(2000, 4000);

		public SoundDef soundMeleeHitPawn;

		public SoundDef soundMeleeHitBuilding;

		public SoundDef soundMeleeMiss;

		[Unsaved(false)]
		private DeathActionWorker deathActionWorkerInt;

		[Unsaved(false)]
		public ThingDef meatDef;

		[Unsaved(false)]
		public ThingDef corpseDef;

		[Unsaved(false)]
		private PawnKindDef cachedAnyPawnKind;

		public bool Humanlike => (int)intelligence >= 2;

		public bool ToolUser => (int)intelligence >= 1;

		public bool Animal
		{
			get
			{
				if (!ToolUser)
				{
					return IsFlesh;
				}
				return false;
			}
		}

		public bool EatsFood => foodType != FoodTypeFlags.None;

		public float FoodLevelPercentageWantEat
		{
			get
			{
				switch (ResolvedDietCategory)
				{
				case DietCategory.NeverEats:
					return 0.3f;
				case DietCategory.Omnivorous:
					return 0.3f;
				case DietCategory.Carnivorous:
					return 0.3f;
				case DietCategory.Ovivorous:
					return 0.4f;
				case DietCategory.Herbivorous:
					return 0.45f;
				case DietCategory.Dendrovorous:
					return 0.45f;
				default:
					throw new InvalidOperationException();
				}
			}
		}

		public DietCategory ResolvedDietCategory
		{
			get
			{
				if (!EatsFood)
				{
					return DietCategory.NeverEats;
				}
				if (Eats(FoodTypeFlags.Tree))
				{
					return DietCategory.Dendrovorous;
				}
				if (Eats(FoodTypeFlags.Meat))
				{
					if (Eats(FoodTypeFlags.VegetableOrFruit) || Eats(FoodTypeFlags.Plant))
					{
						return DietCategory.Omnivorous;
					}
					return DietCategory.Carnivorous;
				}
				if (Eats(FoodTypeFlags.AnimalProduct))
				{
					return DietCategory.Ovivorous;
				}
				return DietCategory.Herbivorous;
			}
		}

		public DeathActionWorker DeathActionWorker
		{
			get
			{
				if (deathActionWorkerInt == null)
				{
					if (deathActionWorkerClass != null)
					{
						deathActionWorkerInt = (DeathActionWorker)Activator.CreateInstance(deathActionWorkerClass);
					}
					else
					{
						deathActionWorkerInt = new DeathActionWorker_Simple();
					}
				}
				return deathActionWorkerInt;
			}
		}

		public FleshTypeDef FleshType
		{
			get
			{
				if (fleshType != null)
				{
					return fleshType;
				}
				return FleshTypeDefOf.Normal;
			}
		}

		public bool IsMechanoid => FleshType == FleshTypeDefOf.Mechanoid;

		public bool IsFlesh => FleshType != FleshTypeDefOf.Mechanoid;

		public ThingDef BloodDef
		{
			get
			{
				if (bloodDef != null)
				{
					return bloodDef;
				}
				if (IsFlesh)
				{
					return ThingDefOf.Filth_Blood;
				}
				return null;
			}
		}

		public bool CanDoHerdMigration
		{
			get
			{
				if (Animal)
				{
					return herdMigrationAllowed;
				}
				return false;
			}
		}

		public PawnKindDef AnyPawnKind
		{
			get
			{
				if (cachedAnyPawnKind == null)
				{
					List<PawnKindDef> allDefsListForReading = DefDatabase<PawnKindDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (allDefsListForReading[i].race.race == this)
						{
							cachedAnyPawnKind = allDefsListForReading[i];
							break;
						}
					}
				}
				return cachedAnyPawnKind;
			}
		}

		public RulePackDef GetNameGenerator(Gender gender)
		{
			if (gender == Gender.Female && nameGeneratorFemale != null)
			{
				return nameGeneratorFemale;
			}
			return nameGenerator;
		}

		public bool CanEverEat(Thing t)
		{
			return CanEverEat(t.def);
		}

		public bool CanEverEat(ThingDef t)
		{
			if (!EatsFood)
			{
				return false;
			}
			if (t.ingestible == null)
			{
				return false;
			}
			if (t.ingestible.preferability == FoodPreferability.Undefined)
			{
				return false;
			}
			return Eats(t.ingestible.foodType);
		}

		public bool Eats(FoodTypeFlags food)
		{
			if (!EatsFood)
			{
				return false;
			}
			return (foodType & food) != 0;
		}

		public void ResolveReferencesSpecial()
		{
			if (useMeatFrom != null)
			{
				meatDef = useMeatFrom.race.meatDef;
			}
			if (useLeatherFrom != null)
			{
				leatherDef = useLeatherFrom.race.leatherDef;
			}
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (soundMeleeHitPawn == null)
			{
				yield return "soundMeleeHitPawn is null";
			}
			if (soundMeleeHitBuilding == null)
			{
				yield return "soundMeleeHitBuilding is null";
			}
			if (soundMeleeMiss == null)
			{
				yield return "soundMeleeMiss is null";
			}
			if (predator && !Eats(FoodTypeFlags.Meat))
			{
				yield return "predator but doesn't eat meat";
			}
			for (int j = 0; j < lifeStageAges.Count; j++)
			{
				for (int i = 0; i < j; i++)
				{
					if (lifeStageAges[i].minAge > lifeStageAges[j].minAge)
					{
						yield return "lifeStages minAges are not in ascending order";
					}
				}
			}
			if (litterSizeCurve != null)
			{
				foreach (string item in litterSizeCurve.ConfigErrors("litterSizeCurve"))
				{
					yield return item;
				}
			}
			if (nameOnTameChance > 0f && nameGenerator == null)
			{
				yield return "can be named, but has no nameGenerator";
			}
			if (Animal && wildness < 0f)
			{
				yield return "is animal but wildness is not defined";
			}
			if (useMeatFrom != null && useMeatFrom.category != ThingCategory.Pawn)
			{
				yield return "tries to use meat from non-pawn " + useMeatFrom;
			}
			if (useMeatFrom != null && useMeatFrom.race.useMeatFrom != null)
			{
				yield return "tries to use meat from " + useMeatFrom + " which uses meat from " + useMeatFrom.race.useMeatFrom;
			}
			if (useLeatherFrom != null && useLeatherFrom.category != ThingCategory.Pawn)
			{
				yield return "tries to use leather from non-pawn " + useLeatherFrom;
			}
			if (useLeatherFrom != null && useLeatherFrom.race.useLeatherFrom != null)
			{
				yield return "tries to use leather from " + useLeatherFrom + " which uses leather from " + useLeatherFrom.race.useLeatherFrom;
			}
			if (Animal && trainability == null)
			{
				yield return "animal has trainability = null";
			}
		}

		public IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef, StatRequest req)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Race".Translate(), parentDef.LabelCap, parentDef.description, 2100);
			if (!parentDef.race.IsMechanoid)
			{
				string text = foodType.ToHumanString().CapitalizeFirst();
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Diet".Translate(), text, "Stat_Race_Diet_Desc".Translate(text), 1500);
			}
			if (req.HasThing && req.Thing is Pawn && (req.Thing as Pawn).needs != null && (req.Thing as Pawn).needs.food != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "NutritationEatenPerDay".Translate(), ((req.Thing as Pawn).needs.food.FoodFallPerTick * 60000f).ToString("0.##"), NutritionEatenPerDayExplanation(req.Thing as Pawn), 4000);
			}
			if (parentDef.race.leatherDef != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "LeatherType".Translate(), parentDef.race.leatherDef.LabelCap, "Stat_Race_LeatherType_Desc".Translate(), 3550, null, new Dialog_InfoCard.Hyperlink[1]
				{
					new Dialog_InfoCard.Hyperlink(parentDef.race.leatherDef)
				});
			}
			if (parentDef.race.Animal || wildness > 0f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Wildness".Translate(), wildness.ToStringPercent(), TrainableUtility.GetWildnessExplanation(parentDef), 2050);
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "HarmedRevengeChance".Translate(), PawnUtility.GetManhunterOnDamageChance(parentDef.race).ToStringPercent(), "HarmedRevengeChanceExplanation".Translate(), 510);
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "TameFailedRevengeChance".Translate(), parentDef.race.manhunterOnTameFailChance.ToStringPercent(), "Stat_Race_Animal_TameFailedRevengeChance_Desc".Translate(), 511);
			}
			if ((int)intelligence < 2 && trainability != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Trainability".Translate(), trainability.LabelCap, "Stat_Race_Trainability_Desc".Translate(), 2500);
			}
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "StatsReport_LifeExpectancy".Translate(), lifeExpectancy.ToStringByStyle(ToStringStyle.Integer), "Stat_Race_LifeExpectancy_Desc".Translate(), 2000);
			if ((int)intelligence < 2 && !parentDef.race.IsMechanoid)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "AnimalFilthRate".Translate(), (PawnUtility.AnimalFilthChancePerCell(parentDef, parentDef.race.baseBodySize) * 1000f).ToString("F2"), "AnimalFilthRateExplanation".Translate(1000.ToString()), 2203);
			}
			if (parentDef.race.Animal)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "PackAnimal".Translate(), packAnimal ? "Yes".Translate() : "No".Translate(), "PackAnimalExplanation".Translate(), 2202);
				if (parentDef.race.nuzzleMtbHours > 0f)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.PawnSocial, "NuzzleInterval".Translate(), Mathf.RoundToInt(parentDef.race.nuzzleMtbHours * 2500f).ToStringTicksToPeriod(), "NuzzleIntervalExplanation".Translate(), 500);
				}
			}
		}

		public static string NutritionEatenPerDayExplanation(Pawn p)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("NoDietCategoryLetter".Translate() + " - " + DietCategory.Omnivorous.ToStringHuman());
			DietCategory[] array = (DietCategory[])Enum.GetValues(typeof(DietCategory));
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != 0 && array[i] != DietCategory.Omnivorous)
				{
					stringBuilder.AppendLine(array[i].ToStringHumanShort() + " - " + array[i].ToStringHuman());
				}
			}
			return "NutritionEatenPerDayTip".Translate(ThingDefOf.MealSimple.GetStatValueAbstract(StatDefOf.Nutrition).ToString("0.##"), stringBuilder.ToString(), p.RaceProps.foodType.ToHumanString());
		}
	}
}