using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class NumericalRuleTests {
    private const float Tolerance = 0.0001f;

    private readonly List<GameObject> createdObjects = new List<GameObject>();
    private readonly List<Object> createdAssets = new List<Object>();
    private readonly List<string> registeredBigObjectNames = new List<string>();

    [SetUp]
    public void SetUp() {
        ResetRuleManager();
    }

    [TearDown]
    public void TearDown() {
        ResetRuleManager();

        foreach (string bigObjectName in registeredBigObjectNames) {
            GameObjectManager.UnregisterBigObject(bigObjectName);
        }

        for (int i = createdObjects.Count - 1; i >= 0; i--) {
            Object.DestroyImmediate(createdObjects[i]);
        }

        for (int i = createdAssets.Count - 1; i >= 0; i--) {
            Object.DestroyImmediate(createdAssets[i]);
        }

        registeredBigObjectNames.Clear();
        createdObjects.Clear();
        createdAssets.Clear();
    }

    [Test]
    public void SmallObjectPropertySetValue_WhenNoRuleExists_UpdatesValueWithinRange() {
        SmallObjectProperty property = new SmallObjectProperty("energy", 5f, 0f, 10f);

        bool result = property.SetValue(7f);

        Assert.That(result, Is.True);
        Assert.That(property.value, Is.EqualTo(7f).Within(Tolerance));
    }

    [Test]
    public void SmallObjectPropertySetValue_WhenValueIsOutOfRange_RejectsModification() {
        SmallObjectProperty property = new SmallObjectProperty("energy", 5f, 0f, 10f);

        bool result = property.SetValue(11f);

        Assert.That(result, Is.False);
        Assert.That(property.value, Is.EqualTo(5f).Within(Tolerance));
    }

    [Test]
    public void NumericalModificationRequest_AddModification_WhenPropertyRepeats_RejectsDuplicateProperty() {
        SmallObjectProperty property = new SmallObjectProperty("energy", 5f, 0f, 10f);
        NumericalModificationRequest request = new NumericalModificationRequest();

        Assert.That(request.AddModification(property, 6f), Is.True);
        Assert.That(request.AddModification(property, 7f), Is.False);
        Assert.That(request.Count, Is.EqualTo(1));
    }

    [Test]
    public void NumericalRuleManagerTryApplyModificationRequest_WhenSingleRegisteredRuleAccepts_ModifiesProperty() {
        SmallObjectProperty property = new SmallObjectProperty("energy", 5f, 0f, 10f);
        RecordingRule rule = new RecordingRule(new[] { property });

        NumericalRuleManager.RegisterRule(rule);

        bool result = property.SetValue(7f);

        Assert.That(result, Is.True);
        Assert.That(property.value, Is.EqualTo(7f).Within(Tolerance));
        Assert.That(rule.CheckValidCallCount, Is.EqualTo(1));
        Assert.That(rule.ApplyCallCount, Is.EqualTo(1));
        Assert.That(rule.LastRequestedValue(property), Is.EqualTo(7f).Within(Tolerance));
    }

    [Test]
    public void NumericalRuleManagerTryApplyModificationRequest_WhenRuleRejects_LeavesValueUnchanged() {
        SmallObjectProperty property = new SmallObjectProperty("energy", 5f, 0f, 10f);
        RecordingRule rule = new RecordingRule(new[] { property }) {
            IsValid = false
        };

        NumericalRuleManager.RegisterRule(rule);

        bool result = property.SetValue(7f);

        Assert.That(result, Is.False);
        Assert.That(property.value, Is.EqualTo(5f).Within(Tolerance));
        Assert.That(rule.CheckValidCallCount, Is.EqualTo(1));
        Assert.That(rule.ApplyCallCount, Is.EqualTo(0));
    }

    [Test]
    public void NumericalRuleManagerRegisterRule_WhenScopesOverlap_RejectsSecondRule() {
        SmallObjectProperty property = new SmallObjectProperty("energy", 5f, 0f, 10f);
        RecordingRule firstRule = new RecordingRule(new[] { property });
        RecordingRule secondRule = new RecordingRule(new[] { property });

        NumericalRuleManager.RegisterRule(firstRule);
        NumericalRuleManager.RegisterRule(secondRule);

        Assert.That(GetRegisteredRules(), Has.Count.EqualTo(1));

        bool result = property.SetValue(7f);

        Assert.That(result, Is.True);
        Assert.That(firstRule.ApplyCallCount, Is.EqualTo(1));
        Assert.That(secondRule.ApplyCallCount, Is.EqualTo(0));
    }

    [Test]
    public void NumericalMaintainConstantRule_CheckValidAndApply_WhenWeightedDeltaNeedsCompensation_DistributesChange() {
        SmallObjectProperty input = new SmallObjectProperty("input", 10f, 0f, 100f);
        SmallObjectProperty outputA = new SmallObjectProperty("outputA", 20f, 0f, 100f);
        SmallObjectProperty outputB = new SmallObjectProperty("outputB", 30f, 0f, 100f);

        NumericalMaintainConstantRule rule = CreateMaintainConstantRule(
            new[] { input, outputA, outputB },
            new[] { 2f, 1f, 3f }
        );

        NumericalRuleManager.RegisterRule(rule);

        bool result = input.SetValue(14f);

        Assert.That(result, Is.True);
        Assert.That(input.value, Is.EqualTo(14f).Within(Tolerance));
        Assert.That(outputA.value, Is.EqualTo(18f).Within(Tolerance));
        Assert.That(outputB.value, Is.EqualTo(28f).Within(Tolerance));
    }

    [Test]
    public void NumericalMaintainConstantRule_WhenDirectWeightedDeltaIsZero_OnlyAppliesRequestedValues() {
        SmallObjectProperty inputA = new SmallObjectProperty("inputA", 10f, 0f, 100f);
        SmallObjectProperty inputB = new SmallObjectProperty("inputB", 20f, 0f, 100f);
        SmallObjectProperty output = new SmallObjectProperty("output", 30f, 0f, 100f);

        NumericalMaintainConstantRule rule = CreateMaintainConstantRule(
            new[] { inputA, inputB, output },
            new[] { 2f, 1f, 3f }
        );

        NumericalRuleManager.RegisterRule(rule);

        NumericalModificationRequest request = new NumericalModificationRequest();
        Assert.That(request.AddModification(inputA, 12f), Is.True);
        Assert.That(request.AddModification(inputB, 16f), Is.True);

        bool result = NumericalRuleManager.TryApplyModificationRequest(request);

        Assert.That(result, Is.True);
        Assert.That(inputA.value, Is.EqualTo(12f).Within(Tolerance));
        Assert.That(inputB.value, Is.EqualTo(16f).Within(Tolerance));
        Assert.That(output.value, Is.EqualTo(30f).Within(Tolerance));
    }

    [Test]
    public void NumericalMaintainConstantRule_WhenUnmodifiedWeightSumIsZero_RejectsModification() {
        SmallObjectProperty input = new SmallObjectProperty("input", 10f, 0f, 100f);
        SmallObjectProperty outputA = new SmallObjectProperty("outputA", 20f, 0f, 100f);
        SmallObjectProperty outputB = new SmallObjectProperty("outputB", 30f, 0f, 100f);

        NumericalMaintainConstantRule rule = CreateMaintainConstantRule(
            new[] { input, outputA, outputB },
            new[] { 1f, 1f, -1f }
        );

        NumericalRuleManager.RegisterRule(rule);

        bool result = input.SetValue(14f);

        Assert.That(result, Is.False);
        Assert.That(input.value, Is.EqualTo(10f).Within(Tolerance));
        Assert.That(outputA.value, Is.EqualTo(20f).Within(Tolerance));
        Assert.That(outputB.value, Is.EqualTo(30f).Within(Tolerance));
    }

    [Test]
    public void NumericalMaintainConstantRule_WhenCompensationExceedsBounds_RejectsModification() {
        SmallObjectProperty input = new SmallObjectProperty("input", 10f, 0f, 100f);
        SmallObjectProperty outputA = new SmallObjectProperty("outputA", 1f, 0f, 2f);
        SmallObjectProperty outputB = new SmallObjectProperty("outputB", 1f, 0f, 2f);

        NumericalMaintainConstantRule rule = CreateMaintainConstantRule(
            new[] { input, outputA, outputB },
            new[] { 1f, 1f, 1f }
        );

        NumericalRuleManager.RegisterRule(rule);

        bool result = input.SetValue(5f);

        Assert.That(result, Is.False);
        Assert.That(input.value, Is.EqualTo(10f).Within(Tolerance));
        Assert.That(outputA.value, Is.EqualTo(1f).Within(Tolerance));
        Assert.That(outputB.value, Is.EqualTo(1f).Within(Tolerance));
    }

    [Test]
    public void NumericalRuleFactoryBuildMaintainConstantRule_CreatesSingleRuleWithAllManagedProperties() {
        PropertyGraph graph = CreateRegisteredPropertyGraph();
        string ruleString = $"2*{graph.BigObjectName}.Past.energy + 3*{graph.BigObjectName}.Present.mass + 5*{graph.BigObjectName}.Present.heat = 100";

        bool result = NumericalRuleFactory.BuildMaintainConstantRule(ruleString, out List<NumericalMaintainConstantRule> rules);

        Assert.That(result, Is.True);
        Assert.That(rules, Has.Count.EqualTo(1));
        Assert.That(rules[0].GetManagedProperties(), Has.Count.EqualTo(3));
        Assert.That(rules[0].GetManagedProperties()[0], Is.SameAs(graph.Energy));
        Assert.That(rules[0].GetManagedProperties()[1], Is.SameAs(graph.Mass));
        Assert.That(rules[0].GetManagedProperties()[2], Is.SameAs(graph.Heat));
    }

    [Test]
    public void NumericalRuleFactoryBuildRule_WhenExpressionIsValid_ReturnsBaseRuleList() {
        PropertyGraph graph = CreateRegisteredPropertyGraph();
        string ruleString = $"1*{graph.BigObjectName}.Past.energy + 1*{graph.BigObjectName}.Present.mass = 30";

        bool result = NumericalRuleFactory.BuildRule(ruleString, out List<NumericalRule> rules);

        Assert.That(result, Is.True);
        Assert.That(rules, Has.Count.EqualTo(1));
        Assert.That(rules[0], Is.TypeOf<NumericalMaintainConstantRule>());
    }

    [TestCase("")]
    [TestCase("1*Missing.Past.energy")]
    [TestCase("1*Missing.Past.energy = 10")]
    public void NumericalRuleFactoryBuildRule_WhenExpressionIsInvalid_ReturnsFalseAndNoRules(string ruleString) {
        bool result = NumericalRuleFactory.BuildRule(ruleString, out List<NumericalRule> rules);

        Assert.That(result, Is.False);
        Assert.That(rules, Is.Empty);
    }

    [Test]
    public void NumericalRuleManagerBuildAndRegisterRules_WithValidRuleString_EnablesPropertyModification() {
        PropertyGraph graph = CreateRegisteredPropertyGraph();
        string ruleString = $"1*{graph.BigObjectName}.Past.energy + 1*{graph.BigObjectName}.Present.mass = 30";

        NumericalRuleManager.BuildAndRegisterRules(ruleString);

        Assert.That(graph.Energy.SetValue(15f), Is.True);
        Assert.That(graph.Mass.value, Is.EqualTo(15f).Within(Tolerance));
    }

    [Test]
    public void NumericalRuleManagerGuardClauses_DoNotThrowForInvalidInputs() {
        Assert.DoesNotThrow(() => NumericalRuleManager.RegisterRule(null));
        Assert.DoesNotThrow(() => NumericalRuleManager.RegisterRules(null));
        Assert.DoesNotThrow(() => NumericalRuleManager.RegisterRules(new List<NumericalRule>()));
        Assert.DoesNotThrow(() => NumericalRuleManager.BuildAndRegisterRules(null));
        Assert.DoesNotThrow(() => NumericalRuleManager.BuildAndRegisterRules(""));
        Assert.DoesNotThrow(() => NumericalRuleManager.TryModifyProperty(null, 1f));
        Assert.DoesNotThrow(() => NumericalRuleManager.TryApplyModificationRequest(null));
    }

    private static NumericalMaintainConstantRule CreateMaintainConstantRule(IReadOnlyList<SmallObjectProperty> properties, IReadOnlyList<float> weights) {
        NumericalMaintainConstantRule rule = new NumericalMaintainConstantRule();
        rule.Initialize(new List<SmallObjectProperty>(properties), new List<float>(weights));
        return rule;
    }

    private static List<NumericalRule> GetRegisteredRules() {
        FieldInfo field = typeof(NumericalRuleManager).GetField("rules", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(field, Is.Not.Null, "未找到 NumericalRuleManager.rules 字段");
        return (List<NumericalRule>)field.GetValue(null);
    }

    private static void ResetRuleManager() {
        List<NumericalRule> rules = GetRegisteredRules();
        rules.Clear();
    }

    private PropertyGraph CreateRegisteredPropertyGraph() {
        string bigObjectName = "Big_" + Guid.NewGuid().ToString("N");
        EnvironmentBigObject bigObject = CreateBigObject(bigObjectName);
        DoorSmallObject pastObject = CreateSmallObject("Past");
        DoorSmallObject presentObject = CreateSmallObject("Present");

        bigObject.pastObject = pastObject;
        bigObject.presentObject = presentObject;
        GameObjectManager.RegisterBigObject(bigObjectName, bigObject);
        registeredBigObjectNames.Add(bigObjectName);

        return new PropertyGraph {
            BigObjectName = bigObjectName,
            Energy = AddProperty(pastObject, "energy", 10f, 0f, 100f),
            Mass = AddProperty(presentObject, "mass", 20f, 0f, 100f),
            Heat = AddProperty(presentObject, "heat", 30f, 0f, 100f)
        };
    }

    private EnvironmentBigObject CreateBigObject(string objectName) {
        GameObject gameObject = new GameObject(objectName);
        createdObjects.Add(gameObject);

        EnvironmentBigObject bigObject = gameObject.AddComponent<EnvironmentBigObject>();
        bigObject.staticData = ScriptableObject.CreateInstance<BigObjectStaticData>();
        createdAssets.Add(bigObject.staticData);
        return bigObject;
    }

    private DoorSmallObject CreateSmallObject(string objectName) {
        GameObject gameObject = new GameObject(objectName);
        createdObjects.Add(gameObject);

        DoorSmallObject smallObject = gameObject.AddComponent<DoorSmallObject>();
        smallObject.staticData = ScriptableObject.CreateInstance<SmallObjectStaticData>();
        createdAssets.Add(smallObject.staticData);
        smallObject.staticData.objectName = objectName;
        smallObject.dynamicState = new SmallObjectDynamicState();
        return smallObject;
    }

    private static SmallObjectProperty AddProperty(
        SmallObject smallObject,
        string propertyName,
        float value,
        float minValue,
        float maxValue
    ) {
        SmallObjectProperty property = new SmallObjectProperty(propertyName, value, minValue, maxValue);
        smallObject.dynamicState.propertyMap[propertyName] = property;
        return property;
    }

    private sealed class RecordingRule : NumericalRule {
        private readonly List<SmallObjectProperty> managedProperties = new List<SmallObjectProperty>();
        private readonly Dictionary<SmallObjectProperty, float> requestedValues = new Dictionary<SmallObjectProperty, float>();

        public bool IsValid = true;
        public int CheckValidCallCount;
        public int ApplyCallCount;

        public RecordingRule(IEnumerable<SmallObjectProperty> properties) {
            if (properties == null) {
                return;
            }

            foreach (SmallObjectProperty property in properties) {
                if (property == null || managedProperties.Contains(property)) {
                    continue;
                }

                managedProperties.Add(property);
            }
        }

        public override IReadOnlyList<SmallObjectProperty> GetManagedProperties() {
            return managedProperties;
        }

        public override bool CheckValid(NumericalModificationRequest request) {
            CheckValidCallCount++;
            requestedValues.Clear();

            if (request != null) {
                foreach (SmallObjectProperty property in managedProperties) {
                    if (request.TryGetModification(property, out float newValue)) {
                        requestedValues[property] = newValue;
                    }
                }
            }

            return IsValid;
        }

        public override void Apply(NumericalModificationRequest request) {
            ApplyCallCount++;

            for (int i = 0; i < managedProperties.Count; i++) {
                SmallObjectProperty property = managedProperties[i];
                if (request != null && request.TryGetModification(property, out float newValue)) {
                    property.SetValueWithoutNotify(newValue);
                }
            }
        }

        public float LastRequestedValue(SmallObjectProperty property) {
            return requestedValues.TryGetValue(property, out float value) ? value : float.NaN;
        }
    }

    private sealed class PropertyGraph {
        public string BigObjectName;
        public SmallObjectProperty Energy;
        public SmallObjectProperty Mass;
        public SmallObjectProperty Heat;
    }
}