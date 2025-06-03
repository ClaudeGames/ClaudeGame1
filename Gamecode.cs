// Farm Royale - Complete Unity Project
// Bevat alle benodigde scripts voor de basis gameplay
// -------------------------------------------------------

// 1. CORE SCRIPTS
// -------------------------------------------------------

// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton patroon
    public static GameManager Instance { get; private set; }
    
    public GameState CurrentState { get; private set; } = GameState.Farm;
    public int PlayerLevel { get; private set; } = 1;
    public int Experience { get; private set; } = 0;
    public int ExperienceToNextLevel { get { return PlayerLevel * 100; } }
    
    // Events
    public delegate void StateChangedHandler(GameState newState);
    public event StateChangedHandler OnStateChanged;
    
    public delegate void PlayerLevelChangedHandler(int newLevel);
    public event PlayerLevelChangedHandler OnPlayerLevelChanged;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeGame()
    {
        // Laad opgeslagen gegevens als deze bestaan
        LoadGameData();
        
        // Initialiseer andere managers
        if (FarmManager.Instance == null)
        {
            GameObject farmManagerObj = new GameObject("Farm Manager");
            farmManagerObj.AddComponent<FarmManager>();
            DontDestroyOnLoad(farmManagerObj);
        }
        
        if (BattleManager.Instance == null)
        {
            GameObject battleManagerObj = new GameObject("Battle Manager");
            battleManagerObj.AddComponent<BattleManager>();
            DontDestroyOnLoad(battleManagerObj);
        }
        
        if (ShopManager.Instance == null)
        {
            GameObject shopManagerObj = new GameObject("Shop Manager");
            shopManagerObj.AddComponent<ShopManager>();
            DontDestroyOnLoad(shopManagerObj);
        }
        
        if (SocialManager.Instance == null)
        {
            GameObject socialManagerObj = new GameObject("Social Manager");
            socialManagerObj.AddComponent<SocialManager>();
            DontDestroyOnLoad(socialManagerObj);
        }
    }
    
    private void LoadGameData()
    {
        // Hier zou je PlayerPrefs of een andere opslagmethode gebruiken
        if (PlayerPrefs.HasKey("PlayerLevel"))
        {
            PlayerLevel = PlayerPrefs.GetInt("PlayerLevel");
            Experience = PlayerPrefs.GetInt("Experience");
        }
    }
    
    private void SaveGameData()
    {
        PlayerPrefs.SetInt("PlayerLevel", PlayerLevel);
        PlayerPrefs.SetInt("Experience", Experience);
        PlayerPrefs.Save();
    }
    
    public void SwitchState(GameState newState)
    {
        CurrentState = newState;
        
        // Laad de juiste scene
        switch (newState)
        {
            case GameState.Farm:
                SceneManager.LoadScene("FarmScene");
                break;
            case GameState.Battle:
                SceneManager.LoadScene("BattleScene");
                break;
            case GameState.Shop:
                SceneManager.LoadScene("ShopScene");
                break;
            case GameState.Social:
                SceneManager.LoadScene("SocialScene");
                break;
        }
        
        // Trigger event voor UI updates
        OnStateChanged?.Invoke(newState);
    }
    
    public void AddExperience(int amount)
    {
        Experience += amount;
        
        // Check voor level up
        while (Experience >= ExperienceToNextLevel)
        {
            Experience -= ExperienceToNextLevel;
            PlayerLevel++;
            OnPlayerLevelChanged?.Invoke(PlayerLevel);
        }
        
        // Sla gegevens op
        SaveGameData();
    }
    
    private void OnApplicationQuit()
    {
        SaveGameData();
    }
    
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveGameData();
        }
    }
}

public enum GameState
{
    Farm,
    Battle,
    Shop,
    Social
}

// ResourceTypes.cs
public enum ResourceType
{
    // Boerderijproducten
    Grain,      // Graan
    Vegetables, // Groenten
    Fruits,     // Fruit
    Milk,       // Melk
    Eggs,       // Eieren
    Wool,       // Wol
    Meat,       // Vlees
    
    // Speciale resources
    Wood,       // Hout
    Stone,      // Steen
    Magic,      // Magische essentie
    
    // Valuta
    Coins,      // Muntjes
    Gems,       // Edelstenen
    
    // Special items
    FarmTickets, // Boerderijtickets
    BattleTickets // Gevechtstickets
}

// DataManager.cs - Voor gegevensopslag en -beheer
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    
    private string savePath;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Application.persistentDataPath + "/farmbattle_save.dat";
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SaveGameData(GameData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);
        
        formatter.Serialize(stream, data);
        stream.Close();
        Debug.Log("Game data saved at " + savePath);
    }
    
    public GameData LoadGameData()
    {
        if (File.Exists(savePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePath, FileMode.Open);
            
            GameData data = formatter.Deserialize(stream) as GameData;
            stream.Close();
            return data;
        }
        else
        {
            Debug.Log("Save file not found at " + savePath);
            return null;
        }
    }
}

// GameData.cs - Class voor opgeslagen gegevens
[System.Serializable]
public class GameData
{
    // Player info
    public int playerLevel;
    public int experience;
    public int coins;
    public int gems;
    
    // Farm data
    public int farmLevel;
    public List<SerializedFarmItem> farmItems;
    public Dictionary<ResourceType, int> resources;
    
    // Battle data
    public int trophies;
    public int arenaLevel;
    public List<SerializedCard> cards;
    public List<int> currentDeck;
    
    // Shop & Social
    public string playerName;
    public int avatarId;
    public string clanName;
    public List<string> friends;
    
    public GameData()
    {
        farmItems = new List<SerializedFarmItem>();
        resources = new Dictionary<ResourceType, int>();
        cards = new List<SerializedCard>();
        currentDeck = new List<int>();
        friends = new List<string>();
    }
}

[System.Serializable]
public class SerializedFarmItem
{
    public string itemType;  // "Crop" of "Animal"
    public string itemName;
    public Vector2Int position;
    public float growthProgress;
    public int waterLevel;
    public int fertilizerLevel;
    public int happinessLevel;  // Voor dieren
    public float lastFeedTime;  // Voor dieren
}

[System.Serializable]
public class SerializedCard
{
    public string cardName;
    public int level;
    public int count; // Aantal kaarten verzameld
}

// 2. FARM SYSTEM
// -------------------------------------------------------

// FarmManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class FarmManager : MonoBehaviour
{
    public static FarmManager Instance { get; private set; }
    
    // Farm eigenschappen
    public int FarmLevel { get; private set; } = 1;
    public int MaxFarmItems { get { return 10 + ((FarmLevel - 1) * 5); } }
    
    // Valuta
    public int Coins { get; private set; } = 100;
    public int Gems { get; private set; } = 5;
    
    // Resources en farm items
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    private List<FarmItem> activeFarmItems = new List<FarmItem>();
    
    // Prefabs
    public Crop[] cropPrefabs;
    public Animal[] animalPrefabs;
    
    // Grid systeem
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    private FarmItem[,] farmGrid;
    
    // Events
    public delegate void ResourceChangedHandler(ResourceType type, int newAmount);
    public event ResourceChangedHandler OnResourceChanged;
    
    public delegate void CurrencyChangedHandler(int newAmount);
    public event CurrencyChangedHandler OnCoinsChanged;
    public event CurrencyChangedHandler OnGemsChanged;
    
    public delegate void FarmLevelChangedHandler(int newLevel);
    public event FarmLevelChangedHandler OnFarmLevelChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFarm();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeFarm()
    {
        // Initialiseer resources
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = 0;
        }
        
        // Initialiseer grid
        farmGrid = new FarmItem[gridWidth, gridHeight];
        
        // Laad opgeslagen farm data
        LoadFarmData();
    }
    
    private void LoadFarmData()
    {
        // In een echte implementatie zou je dit laden van DataManager
        GameData data = DataManager.Instance?.LoadGameData();
        
        if (data != null)
        {
            FarmLevel = data.farmLevel;
            Coins = data.coins;
            Gems = data.gems;
            
            // Laad resources
            resources = data.resources;
            
            // Laad farm items
            foreach (var serializedItem in data.farmItems)
            {
                SpawnSavedFarmItem(serializedItem);
            }
        }
    }
    
    private void SpawnSavedFarmItem(SerializedFarmItem savedItem)
    {
        // Implementeer het spawnen van opgeslagen items
        // In een echte implementatie zou je een prefab zoeken op basis van de naam
        // en deze instellen met de opgeslagen waarden
    }
    
    private void Update()
    {
        // Update alle farm items
        foreach (var item in activeFarmItems)
        {
            item.UpdateGrowth(Time.deltaTime);
        }
    }
    
    public bool PlaceFarmItem(string itemName, Vector2Int position)
    {
        // Check of positie binnen grid valt
        if (position.x < 0 || position.x >= gridWidth || 
            position.y < 0 || position.y >= gridHeight)
        {
            return false;
        }
        
        // Check of positie leeg is
        if (farmGrid[position.x, position.y] != null)
        {
            return false;
        }
        
        // Check aantal items
        if (activeFarmItems.Count >= MaxFarmItems)
        {
            return false;
        }
        
        // Zoek prefab
        FarmItem prefabToSpawn = null;
        
        // Zoek in gewassen
        foreach (var crop in cropPrefabs)
        {
            if (crop.itemName == itemName)
            {
                prefabToSpawn = crop;
                break;
            }
        }
        
        // Zoek in dieren
        if (prefabToSpawn == null)
        {
            foreach (var animal in animalPrefabs)
            {
                if (animal.itemName == itemName)
                {
                    prefabToSpawn = animal;
                    break;
                }
            }
        }
        
        if (prefabToSpawn == null)
        {
            Debug.LogError("Farm item not found: " + itemName);
            return false;
        }
        
        // Spawn item
        FarmItem newItem = Instantiate(prefabToSpawn, 
                                     new Vector3(position.x, position.y, 0), 
                                     Quaternion.identity);
        newItem.Initialize();
        
        // Voeg toe aan beheer
        activeFarmItems.Add(newItem);
        farmGrid[position.x, position.y] = newItem;
        
        return true;
    }
    
    public void HarvestFarmItem(Vector2Int position)
    {
        if (position.x < 0 || position.x >= gridWidth || 
            position.y < 0 || position.y >= gridHeight)
        {
            return;
        }
        
        FarmItem item = farmGrid[position.x, position.y];
        if (item != null && item.CanHarvest())
        {
            // Harvest en verzamel resources
            Dictionary<ResourceType, int> harvestedResources = item.Harvest();
            
            foreach (var resource in harvestedResources)
            {
                AddResource(resource.Key, resource.Value);
            }
            
            // Voor sommigen items (zoals dieren) willen we ze niet verwijderen na de oogst
            if (item.destroyOnHarvest)
            {
                RemoveFarmItem(position);
            }
        }
    }
    
    public void RemoveFarmItem(Vector2Int position)
    {
        if (position.x < 0 || position.x >= gridWidth || 
            position.y < 0 || position.y >= gridHeight)
        {
            return;
        }
        
        FarmItem item = farmGrid[position.x, position.y];
        if (item != null)
        {
            activeFarmItems.Remove(item);
            farmGrid[position.x, position.y] = null;
            Destroy(item.gameObject);
        }
    }
    
    // Resource beheer
    public void AddResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
            resources[type] = 0;
            
        resources[type] += amount;
        
        // Check voor speciale valuta resources
        if (type == ResourceType.Coins)
        {
            Coins += amount;
            OnCoinsChanged?.Invoke(Coins);
        }
        else if (type == ResourceType.Gems)
        {
            Gems += amount;
            OnGemsChanged?.Invoke(Gems);
        }
        else
        {
            // Normale resource
            OnResourceChanged?.Invoke(type, resources[type]);
        }
    }
    
    public bool UseResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type) || resources[type] < amount)
            return false;
        
        // Speciale valuta resources
        if (type == ResourceType.Coins)
        {
            if (Coins < amount) return false;
            Coins -= amount;
            OnCoinsChanged?.Invoke(Coins);
        }
        else if (type == ResourceType.Gems)
        {
            if (Gems < amount) return false;
            Gems -= amount;
            OnGemsChanged?.Invoke(Gems);
        }
        else
        {
            // Normale resource
            resources[type] -= amount;
            OnResourceChanged?.Invoke(type, resources[type]);
        }
        
        return true;
    }
    
    public int GetResourceAmount(ResourceType type)
    {
        if (type == ResourceType.Coins)
            return Coins;
        else if (type == ResourceType.Gems)
            return Gems;
            
        if (!resources.ContainsKey(type))
            return 0;
            
        return resources[type];
    }
    
    public bool UpgradeFarm()
    {
        int cost = FarmLevel * 1000; // Kosten verhogen met elk level
        
        if (Coins < cost)
            return false;
            
        UseResource(ResourceType.Coins, cost);
        FarmLevel++;
        
        OnFarmLevelChanged?.Invoke(FarmLevel);
        return true;
    }
    
    public void SaveFarmData()
    {
        // In een echte implementatie zou je dit opslaan via DataManager
        GameData data = new GameData();
        
        data.farmLevel = FarmLevel;
        data.coins = Coins;
        data.gems = Gems;
        data.resources = resources;
        
        // Serialize farm items
        data.farmItems = new List<SerializedFarmItem>();
        foreach (var item in activeFarmItems)
        {
            SerializedFarmItem serializedItem = new SerializedFarmItem();
            serializedItem.itemName = item.itemName;
            serializedItem.position = new Vector2Int(Mathf.RoundToInt(item.transform.position.x), 
                                                   Mathf.RoundToInt(item.transform.position.y));
            serializedItem.growthProgress = item.currentGrowthProgress;
            
            // Type-specifieke gegevens
            if (item is Crop crop)
            {
                serializedItem.itemType = "Crop";
                serializedItem.waterLevel = crop.currentWaterLevel;
                serializedItem.fertilizerLevel = crop.currentFertilizerLevel;
            }
            else if (item is Animal animal)
            {
                serializedItem.itemType = "Animal";
                serializedItem.happinessLevel = animal.happinessLevel;
                serializedItem.lastFeedTime = animal.lastFeedTime;
            }
            
            data.farmItems.Add(serializedItem);
        }
        
        DataManager.Instance?.SaveGameData(data);
    }
}

// FarmItem.cs - Basis voor boerderij objecten
using System.Collections.Generic;
using UnityEngine;

public abstract class FarmItem : MonoBehaviour
{
    public string itemName;
    public int baseValue;
    public float growthTime;
    public Sprite icon;
    public bool destroyOnHarvest = false;
    
    public float currentGrowthProgress = 0f;
    protected bool isFullyGrown = false;
    
    // Visuele feedback
    public SpriteRenderer spriteRenderer;
    public Sprite[] growthStages;
    
    public virtual void Initialize()
    {
        currentGrowthProgress = 0f;
        isFullyGrown = false;
        UpdateVisuals();
    }
    
    public virtual void UpdateGrowth(float deltaTime)
    {
        if (isFullyGrown) return;
        
        currentGrowthProgress += deltaTime;
        
        // Update visuals based on growth stage
        UpdateVisuals();
        
        if (currentGrowthProgress >= growthTime)
        {
            isFullyGrown = true;
            OnFullyGrown();
        }
    }
    
    protected virtual void UpdateVisuals()
    {
        if (spriteRenderer != null && growthStages != null && growthStages.Length > 0)
        {
            // Bereken huidige groeistadium
            float growthPercentage = Mathf.Clamp01(currentGrowthProgress / growthTime);
            int stageIndex = Mathf.FloorToInt(growthPercentage * (growthStages.Length - 1));
            
            // Update sprite
            spriteRenderer.sprite = growthStages[stageIndex];
        }
    }
    
    protected virtual void OnFullyGrown() { }
    
    public virtual bool CanHarvest()
    {
        return isFullyGrown;
    }
    
    public virtual Dictionary<ResourceType, int> Harvest()
    {
        Dictionary<ResourceType, int> result = new Dictionary<ResourceType, int>();
        
        if (!isFullyGrown) return result;
        
        isFullyGrown = false;
        currentGrowthProgress = 0f;
        
        // Reset visual
        UpdateVisuals();
        
        return result; // Subklassen moeten deze overschrijven om resources terug te geven
    }
}

// Crop.cs - Specifieke implementatie voor gewassen
using System.Collections.Generic;
using UnityEngine;

public class Crop : FarmItem
{
    public ResourceType resourceType;
    public int waterRequirement;
    public int fertilizerBonus;
    
    public int currentWaterLevel = 0;
    public int currentFertilizerLevel = 0;
    
    // Visuele feedback voor water niveau
    public GameObject waterIndicator;
    
    public override void Initialize()
    {
        base.Initialize();
        currentWaterLevel = 0;
        currentFertilizerLevel = 0;
        destroyOnHarvest = true; // De meeste gewassen worden verwijderd na oogst
        
        UpdateWaterVisual();
    }
    
    public void AddWater(int amount)
    {
        currentWaterLevel = Mathf.Min(currentWaterLevel + amount, waterRequirement);
        UpdateWaterVisual();
    }
    
    private void UpdateWaterVisual()
    {
        // Update de visuele indicator voor waterniveau
        if (waterIndicator != null)
        {
            float waterPercentage = (float)currentWaterLevel / waterRequirement;
            // Bijvoorbeeld: schaal of kleur aanpassen
        }
    }
    
    public void AddFertilizer(int amount)
    {
        currentFertilizerLevel += amount;
    }
    
    public override void UpdateGrowth(float deltaTime)
    {
        // Groeien alleen als er voldoende water is
        if (currentWaterLevel >= waterRequirement)
        {
            base.UpdateGrowth(deltaTime);
            
            // Verbruik wat water tijdens de groei
            currentWaterLevel -= Time.deltaTime * 0.1f;
            UpdateWaterVisual();
        }
    }
    
    public override Dictionary<ResourceType, int> Harvest()
    {
        Dictionary<ResourceType, int> result = base.Harvest();
        
        // Bereken opbrengst met bonus op basis van meststof
        float fertilizerMultiplier = 1f + (currentFertilizerLevel * 0.1f);
        int amount = Mathf.RoundToInt(baseValue * fertilizerMultiplier);
        
        result.Add(resourceType, amount);
        
        // Reset voor nieuw planten
        currentWaterLevel = 0;
        currentFertilizerLevel = 0;
        UpdateWaterVisual();
        
        return result;
    }
}

// Animal.cs - Specifieke implementatie voor dieren
using System.Collections.Generic;
using UnityEngine;

public class Animal : FarmItem
{
    public ResourceType primaryResource;
    public ResourceType secondaryResource;
    public float secondaryResourceChance;
    
    public int happinessLevel = 5; // 1-10 schaal
    public float feedCooldown = 12f; // Uren tussen voedingen
    public float lastFeedTime = 0f;
    
    // Visuele feedback
    public GameObject happinessIndicator;
    
    public override void Initialize()
    {
        base.Initialize();
        destroyOnHarvest = false; // Dieren blijven na oogst
        happinessLevel = 5;
        lastFeedTime = Time.time;
        
        UpdateHappinessVisual();
    }
    
    public void Feed()
    {
        lastFeedTime = Time.time;
        happinessLevel = Mathf.Min(happinessLevel + 2, 10);
        UpdateHappinessVisual();
    }
    
    private void UpdateHappinessVisual()
    {
        // Update visuele indicator voor geluk
        if (happinessIndicator != null)
        {
            float happinessPercentage = happinessLevel / 10f;
            // Bijvoorbeeld: emoticon of kleur veranderen
        }
    }
    
    public override void UpdateGrowth(float deltaTime)
    {
        base.UpdateGrowth(deltaTime);
        
        // Controleer of het dier voeding nodig heeft
        float hoursSinceLastFeed = (Time.time - lastFeedTime) / 3600f;
        if (hoursSinceLastFeed > feedCooldown)
        {
            // Verminder geluk als het dier niet op tijd wordt gevoed
            happinessLevel = Mathf.Max(1, happinessLevel - (int)(deltaTime * 0.01f));
            UpdateHappinessVisual();
        }
    }
    
    public override Dictionary<ResourceType, int> Harvest()
    {
        Dictionary<ResourceType, int> result = base.Harvest();
        
        // Gelukkigere dieren geven betere opbrengsten
        float happinessMultiplier = 0.5f + (happinessLevel * 0.1f);
        int amount = Mathf.RoundToInt(baseValue * happinessMultiplier);
        
        result.Add(primaryResource, amount);
        
        // Kans op secundaire resource
        if (UnityEngine.Random.value <= secondaryResourceChance * (happinessLevel / 10f))
        {
            result.Add(secondaryResource, Mathf.RoundToInt(baseValue * 0.3f));
        }
        
        return result;
    }
}

// 3. BATTLE SYSTEM
// -------------------------------------------------------

// BattleManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    
    // Battle eigenschappen
    public int Trophies { get; private set; } = 0;
    public int ArenaLevel { get; private set; } = 1;
    
    // Battle systeem
    private List<BattleCard> availableCards = new List<BattleCard>();
    private List<BattleCard> currentDeck = new List<BattleCard>();
    private int maxDeckSize = 8;
    
    // Battle status
    private bool inBattle = false;
    private float battleTime = 180f; // 3 minuten
    private float currentBattleTime = 0f;
    private int playerTowers = 3;
    private int enemyTowers = 3;
    
    // Elixir systeem
    private float maxElixir = 10f;
    private float currentElixir = 5f;
    private float elixirRegenerationRate = 0.35f; // per seconde
    
    // Card spawning
    private Queue<BattleCard> cardQueue = new Queue<BattleCard>();
    
    // Events
    public delegate void TrophiesChangedHandler(int newAmount);
    public event TrophiesChangedHandler OnTrophiesChanged;
    
    public delegate void ArenaChangedHandler(int newArena);
    public event ArenaChangedHandler OnArenaChanged;
    
    public delegate void ElixirChangedHandler(float currentElixir, float maxElixir);
    public event ElixirChangedHandler OnElixirChanged;
    
    public delegate void BattleStateChangedHandler(bool inBattle);
    public event BattleStateChangedHandler OnBattleStateChanged;
    
    public delegate void BattleTimeChangedHandler(float remainingTime);
    public event BattleTimeChangedHandler OnBattleTimeChanged;
    
    public delegate void TowerDestroyedHandler(bool isPlayerTower, int remaining);
    public event TowerDestroyedHandler OnTowerDestroyed;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeBattleSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeBattleSystem()
    {
        // Laad beschikbare kaarten
        LoadCardData();
        
        // Laad spelersdeck
        LoadDeckData();
    }
    
    private void LoadCardData()
    {
        // In een echte implementatie zou je dit laden van DataManager
        GameData data = DataManager.Instance?.LoadGameData();
        
        if (data != null)
        {
            Trophies = data.trophies;
            ArenaLevel = data.arenaLevel;
            
            // Laad kaarten
            // In een werkelijke implementatie zou je hier kaarten uit een data bestand laden
            // en ze toevoegen aan de beschikbare kaarten lijst
        }
        else
        {
            // Default kaarten toevoegen voor nieuwe spelers
            CreateDefaultCards();
        }
    }
    
    private void CreateDefaultCards()
    {
        // In een werkelijke implementatie zou je dit met echte kaartgegevens doen
        for (int i = 0; i < 12; i++)
        {
            BattleCard card = new BattleCard();
            card.cardId = i;
            card.cardName = "Starter Card " + i;
            card.level = 1;
            card.manaCost = UnityEngine.Random.Range(1, 6);
            
            if (i < 4)
                card.cardType = CardType.Troop;
            else if (i < 8)
                card.cardType = CardType.Spell;
            else
                card.cardType = CardType.Building;
                
            availableCards.Add(card);
            
            // Voeg de eerste 8 kaarten toe aan het spelersdeck
            if (i < 8)
            {
                currentDeck.Add(card);
            }
        }
    }
    
    private void LoadDeckData()
    {
        // In een echte implementatie zou je dit laden van DataManager
        GameData data = DataManager.Instance?.LoadGameData();
        
        if (data != null && data.currentDeck != null && data.currentDeck.Count > 0)
        {
            // Laad opgeslagen deck
            currentDeck.Clear();
            foreach (int cardId in data.currentDeck)
            {
                BattleCard card = availableCards.Find(c => c.cardId == cardId);
                if (card != null)
                {
                    currentDeck.Add(card);
                }
            }
        }
        
        // Zorg dat het deck vol is
        while (currentDeck.Count < maxDeckSize && availableCards.Count > 0)
        {
            BattleCard randomCard = availableCards[UnityEngine.Random.Range(0, availableCards.Count)];
            if (!currentDeck.Contains(randomCard))
            {
                currentDeck.Add(randomCard);
            }
        }
        
        // Initialiseer kaartenwachtrij
        ShuffleDeck();
    }
    
    private void ShuffleDeck()
    {
        // Schud het deck en vul de wachtrij
        cardQueue.Clear();
        List<BattleCard> shuffledDeck = new List<BattleCard>(currentDeck);
        
        for (int i = 0; i < shuffledDeck.Count; i++)
        {
            BattleCard temp = shuffledDeck[i];
            int randomIndex = UnityEngine.Random.Range(i, shuffledDeck.Count);
            shuffledDeck[i] = shuffledDeck[randomIndex];
            shuffledDeck[randomIndex] = temp;
        }
        
        foreach (var card in shuffledDeck)
        {
            cardQueue.Enqueue(card);
        }
    }
    
    private void Start()
    {
        // Start elixir regeneratie
        StartCoroutine(RegenerateElixir());
    }
    
    private IEnumerator RegenerateElixir()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
            if (currentElixir < maxElixir)
            {
                currentElixir = Mathf.Min(currentElixir + elixirRegenerationRate, maxElixir);
                OnElixirChanged?.Invoke(currentElixir, maxElixir);
            }
        }
    }
    
    public void StartBattle()
    {
        if (inBattle) return;
        
        inBattle = true;
        currentBattleTime = battleTime;
        playerTowers = 3;
        enemyTowers = 3;
        currentElixir = 5f;
        
        // Reset deck
        ShuffleDeck();
        
        OnBattleStateChanged?.Invoke(true);
        StartCoroutine(BattleTimer());
    }
    
    private IEnumerator BattleTimer()
    {
        while (currentBattleTime > 0 && inBattle)
        {
            currentBattleTime -= Time.deltaTime;
            OnBattleTimeChanged?.Invoke(currentBattleTime);
            
            // Dubbele elixir in de laatste minuut
            if (currentBattleTime <= 60f)
            {
                elixirRegenerationRate = 0.7f; // Dubbele snelheid
            }
            
            yield return null;
        }
        
        if (inBattle)
        {
            EndBattle(false); // Tijd verlopen = gelijkspel/verlies
        }
    }
    
    public bool PlayCard(int cardIndex, Vector2 position)
    {
        if (!inBattle || cardIndex < 0 || cardIndex >= currentDeck.Count)
            return false;
            
        BattleCard card = currentDeck[cardIndex];
        
        if (card.manaCost > currentElixir)
            return false;
            
        // Verbruik elixir
        currentElixir -= card.manaCost;
        OnElixirChanged?.Invoke(currentElixir, maxElixir);
        
        // Spawn de kaart in de arena
        SpawnCardInArena(card, position);
        
        // Vervang de kaart in de hand
        if (cardQueue.Count > 0)
        {
            currentDeck[cardIndex] = cardQueue.Dequeue();
        }
        else
        {
            // Als de wachtrij leeg is, schud het deck opnieuw
            ShuffleDeck();
            if (cardQueue.Count > 0)
            {
                currentDeck[cardIndex] = cardQueue.Dequeue();
            }
        }
        
        return true;
    }
    
    private void SpawnCardInArena(BattleCard card, Vector2 position)
    {
        // In een echte implementatie zou je hier de kaart spawnen in de arena
        // met de juiste prefab en eigenschappen
        
        switch (card.cardType)
        {
            case CardType.Troop:
                SpawnTroop(card, position);
                break;
            case CardType.Spell:
                CastSpell(card, position);
                break;
            case CardType.Building:
                PlaceBuilding(card, position);
                break;
            case CardType.FarmAnimal:
                SpawnFarmAnimal(card, position);
                break;
        }
    }
    
    private void SpawnTroop(BattleCard card, Vector2 position)
    {
        // Spawn een troep unit
        Debug.Log($"Spawning troop: {card.cardName} at {position}");
        
        // In een echte implementatie:
        // GameObject troopPrefab = Resources.Load<GameObject>("Troops/" + card.cardName);
        // GameObject troop = Instantiate(troopPrefab, position, Quaternion.identity);
        // TroopController controller = troop.GetComponent<TroopController>();
        // controller.Initialize(card);
    }
    
    private void CastSpell(BattleCard card, Vector2 position)
    {
        // Cast een spreuk
        Debug.Log($"Casting spell: {card.cardName} at {position}");
        
        // In een echte implementatie zou je hier de spreukeffecten toepassen
    }
    
    private void PlaceBuilding(BattleCard card, Vector2 position)
    {
        // Plaats een gebouw
        Debug.Log($"Placing building: {card.cardName} at {position}");
        
        // In een echte implementatie:
        // GameObject buildingPrefab = Resources.Load<GameObject>("Buildings/" + card.cardName);
        // GameObject building = Instantiate(buildingPrefab, position, Quaternion.identity);
    }
    
    private void SpawnFarmAnimal(BattleCard card, Vector2 position)
    {
        // Spawn een boerderij dier (speciale kaarten)
        Debug.Log($"Spawning farm animal: {card.cardName} at {position}");
        
        // Deze zouden speciale vaardigheden kunnen hebben gerelateerd aan de boerderij
    }
    
    public void DestroyPlayerTower()
    {
        playerTowers--;
        OnTowerDestroyed?.Invoke(true, playerTowers);
        
        if (playerTowers <= 0)
        {
            EndBattle(false); // Speler verliest
        }
    }
    
    public void DestroyEnemyTower()
    {
        enemyTowers--;
        OnTowerDestroyed?.Invoke(false, enemyTowers);
        
        if (enemyTowers <= 0)
        {
            EndBattle(true); // Speler wint
        }
    }
    
    public void EndBattle(bool playerWon)
    {
        if (!inBattle) return;
        
        inBattle = false;
        elixirRegenerationRate = 0.35f; // Reset elixir regeneratie
        
        OnBattleStateChanged?.Invoke(false);
        
        if (playerWon)
        {
            WinBattle();
        }
        else
        {
            LoseBattle();
        }
    }
    
    private void WinBattle()
    {
        // Trofeeën toevoegen
        int trophyGain = UnityEngine.Random.Range(25, 35);
        Trophies += trophyGain;
        OnTrophiesChanged?.Invoke(Trophies);
        
        // Check voor arena vooruitgang
        CheckArenaProgress();
        
        // Beloningen
        int coinReward = 50 + (ArenaLevel * 10);
        FarmManager.Instance?.AddResource(ResourceType.Coins, coinReward);
        
        // Kans op edelstenen
        if (UnityEngine.Random.value < 0.1f)
        {
            int gemReward = UnityEngine.Random.Range(1, 3);
            FarmManager.Instance?.AddResource(ResourceType.Gems, gemReward);
        }
        
        // Ervaring toevoegen
        GameManager.Instance?.AddExperience(50);
        
        Debug.Log($"Battle won! +{trophyGain} trophies, +{coinReward} coins");
    }
    
    private void LoseBattle()
    {
        // Trofeeën verliezen
        int trophyLoss = UnityEngine.Random.Range(15, 25);
        Trophies = Mathf.Max(0, Trophies - trophyLoss);
        OnTrophiesChanged?.Invoke(Trophies);
        
        // Check voor arena daling
        CheckArenaProgress();
        
        // Kleine beloning voor deelname
        int coinReward = 10;
        FarmManager.Instance?.AddResource(ResourceType.Coins, coinReward);
        
        // Minder ervaring
        GameManager.Instance?.AddExperience(10);
        
        Debug.Log($"Battle lost! -{trophyLoss} trophies, +{coinReward} coins");
    }
    
    private void CheckArenaProgress()
    {
        int newArena = (Trophies / 300) + 1; // Elke 300 trofeeën = nieuwe arena
        
        if (newArena != ArenaLevel)
        {
            ArenaLevel = newArena;
            OnArenaChanged?.Invoke(ArenaLevel);
            
            if (newArena > ArenaLevel)
            {
                Debug.Log($"Promoted to Arena {ArenaLevel}!");
                // Unlock nieuwe kaarten of andere beloningen
            }
            else
            {
                Debug.Log($"Demoted to Arena {ArenaLevel}");
            }
        }
    }
    
    // Kaart beheer
    public void AddCard(BattleCard card)
    {
        availableCards.Add(card);
    }
    
    public void UpgradeCard(int cardId)
    {
        BattleCard card = availableCards.Find(c => c.cardId == cardId);
        if (card != null)
        {
            int upgradeCost = card.CalculateUpgradeCost();
            
            if (FarmManager.Instance?.GetResourceAmount(ResourceType.Coins) >= upgradeCost)
            {
                FarmManager.Instance?.UseResource(ResourceType.Coins, upgradeCost);
                card.Upgrade();
                Debug.Log($"Upgraded {card.cardName} to level {card.level}");
            }
        }
    }
    
    public bool ChangeDeck(List<int> newDeckCardIds)
    {
        if (newDeckCardIds.Count != maxDeckSize)
            return false;
            
        List<BattleCard> newDeck = new List<BattleCard>();
        
        foreach (int cardId in newDeckCardIds)
        {
            BattleCard card = availableCards.Find(c => c.cardId == cardId);
            if (card == null)
                return false;
                
            newDeck.Add(card);
        }
        
        currentDeck = newDeck;
        ShuffleDeck();
        return true;
    }
    
    // Getters
    public List<BattleCard> GetAvailableCards() => availableCards;
    public List<BattleCard> GetCurrentDeck() => currentDeck;
    public float GetCurrentElixir() => currentElixir;
    public bool IsInBattle() => inBattle;
    public float GetRemainingBattleTime() => currentBattleTime;
}

// BattleCard.cs
[System.Serializable]
public class BattleCard
{
    public int cardId;
    public string cardName;
    public int level = 1;
    public int manaCost;
    public CardType cardType;
    public string description;
    
    // Troop/Building stats
    public int damage;
    public int health;
    public float attackSpeed;
    public float range;
    public float movementSpeed;
    
    // Special eigenschappen
    public bool isFlying = false;
    public bool splashDamage = false;
    public float splashRadius = 0f;
    
    // Kosten en upgrading
    public int count = 1; // Aantal kaarten verzameld
    public int requiredForUpgrade = 2;
    
    public void Upgrade()
    {
        if (count >= requiredForUpgrade)
        {
            level++;
            count -= requiredForUpgrade;
            requiredForUpgrade = Mathf.RoundToInt(requiredForUpgrade * 1.5f);
            
            // Verhoog stats
            damage = Mathf.RoundToInt(damage * 1.1f);
            health = Mathf.RoundToInt(health * 1.1f);
        }
    }
    
    public int CalculateUpgradeCost()
    {
        return 100 * level * level;
    }
    
    public bool CanUpgrade()
    {
        return count >= requiredForUpgrade;
    }
}

public enum CardType
{
    Troop,
    Spell,
    Building,
    FarmAnimal
}

// 4. SHOP SYSTEM
// -------------------------------------------------------

// ShopManager.cs
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    
    // Shop items
    private List<ShopItem> availableItems = new List<ShopItem>();
    private List<ShopItem> dailyOffers = new List<ShopItem>();
    
    // Timers
    private float dailyOfferResetTime = 86400f; // 24 uur
    private float lastDailyOfferReset = 0f;
    
    // Events
    public delegate void ShopItemPurchasedHandler(ShopItem item);
    public event ShopItemPurchasedHandler OnItemPurchased;
    
    public delegate void DailyOffersUpdatedHandler();
    public event DailyOffersUpdatedHandler OnDailyOffersUpdated;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeShop();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeShop()
    {
        CreateShopItems();
        GenerateDailyOffers();
    }
    
    private void Update()
    {
        // Check of dagelijkse aanbiedingen moeten worden gereset
        if (Time.time - lastDailyOfferReset >= dailyOfferResetTime)
        {
            GenerateDailyOffers();
        }
    }
    
    private void CreateShopItems()
    {
        // Kaartpakketten
        availableItems.Add(new ShopItem
        {
            itemId = "card_pack_basic",
            itemName = "Basis Kaartpakket",
            description = "5 willekeurige kaarten",
            coinCost = 100,
            gemCost = 0,
            itemType = ShopItemType.CardPack,
            quantity = 5
        });
        
        availableItems.Add(new ShopItem
        {
            itemId = "card_pack_premium",
            itemName = "Premium Kaartpakket",
            description = "10 willekeurige kaarten, hogere kans op zeldzame kaarten",
            coinCost = 0,
            gemCost = 10,
            itemType = ShopItemType.CardPack,
            quantity = 10
        });
        
        // Boerderijitems
        availableItems.Add(new ShopItem
        {
            itemId = "wheat_seeds",
            itemName = "Tarwezaden",
            description = "Pakkket van 10 tarwezaden",
            coinCost = 50,
            gemCost = 0,
            itemType = ShopItemType.FarmItem,
            quantity = 10
        });
        
        availableItems.Add(new ShopItem
        {
            itemId = "cow",
            itemName = "Koe",
            description = "Een mooie melkkoe voor je boerderij",
            coinCost = 500,
            gemCost = 0,
            itemType = ShopItemType.FarmItem,
            quantity = 1
        });
        
        // Valuta
        availableItems.Add(new ShopItem
        {
            itemId = "coin_pack_small",
            itemName = "Kleine Muntenzak",
            description = "1000 extra munten",
            coinCost = 0,
            gemCost = 5,
            itemType = ShopItemType.Currency,
            quantity = 1000
        });
        
        availableItems.Add(new ShopItem
        {
            itemId = "gem_pack_small",
            itemName = "Kleine Edelsteenzak",
            description = "50 edelstenen",
            coinCost = 0,
            gemCost = 0,
            realMoneyCost = 2.99f,
            itemType = ShopItemType.Currency,
            quantity = 50
        });
        
        // Decoraties
        availableItems.Add(new ShopItem
        {
            itemId = "fence_wooden",
            itemName = "Houten Hek",
            description = "Decoratief hek voor je boerderij",
            coinCost = 25,
            gemCost = 0,
            itemType = ShopItemType.Decoration,
            quantity = 1
        });
        
        // Boosts
        availableItems.Add(new ShopItem
        {
            itemId = "growth_boost",
            itemName = "Groeiversneller",
            description = "Versnelt gewasgroei met 50% voor 24 uur",
            coinCost = 0,
            gemCost = 20,
            itemType = ShopItemType.Boost,
            quantity = 1
        });
    }
    
    private void GenerateDailyOffers()
    {
        dailyOffers.Clear();
        
        // Selecteer willekeurige items voor dagelijkse aanbiedingen
        List<ShopItem> tempItems = new List<ShopItem>(availableItems);
        
        for (int i = 0; i < 3; i++) // 3 dagelijkse aanbiedingen
        {
            if (tempItems.Count == 0) break;
            
            int randomIndex = UnityEngine.Random.Range(0, tempItems.Count);
            ShopItem originalItem = tempItems[randomIndex];
            tempItems.RemoveAt(randomIndex);
            
            // Maak een kopie met korting
            ShopItem discountedItem = new ShopItem
            {
                itemId = originalItem.itemId + "_daily",
                itemName = originalItem.itemName + " (Dagelijks)",
                description = originalItem.description + " - 50% KORTING!",
                coinCost = Mathf.RoundToInt(originalItem.coinCost * 0.5f),
                gemCost = Mathf.RoundToInt(originalItem.gemCost * 0.5f),
                realMoneyCost = originalItem.realMoneyCost * 0.5f,
                itemType = originalItem.itemType,
                quantity = originalItem.quantity,
                isDailyOffer = true
            };
            
            dailyOffers.Add(discountedItem);
        }
        
        lastDailyOfferReset = Time.time;
        OnDailyOffersUpdated?.Invoke();
        
        Debug.Log("Daily offers updated!");
    }
    
    public bool PurchaseItem(string itemId)
    {
        ShopItem item = FindItem(itemId);
        if (item == null)
        {
            Debug.LogError("Item not found: " + itemId);
            return false;
        }
        
        // Check of speler het kan betalen
        if (!CanAfford(item))
        {
            Debug.Log("Cannot afford item: " + item.itemName);
            return false;
        }
        
        // Verwerk betaling
        ProcessPayment(item);
        
        // Geef item aan speler
        GiveItemToPlayer(item);
        
        OnItemPurchased?.Invoke(item);
        return true;
    }
    
    private ShopItem FindItem(string itemId)
    {
        // Zoek eerst in dagelijkse aanbiedingen
        foreach (var item in dailyOffers)
        {
            if (item.itemId == itemId)
                return item;
        }
        
        // Zoek dan in gewone items
        foreach (var item in availableItems)
        {
            if (item.itemId == itemId)
                return item;
        }
        
        return null;
    }
    
    private bool CanAfford(ShopItem item)
    {
        if (item.realMoneyCost > 0)
        {
            // Real money purchases zouden via platform stores gaan
            return true; // Simplified voor dit voorbeeld
        }
        
        bool canAffordCoins = item.coinCost == 0 || 
                             FarmManager.Instance.GetResourceAmount(ResourceType.Coins) >= item.coinCost;
        
        bool canAffordGems = item.gemCost == 0 || 
                            FarmManager.Instance.GetResourceAmount(ResourceType.Gems) >= item.gemCost;
        
        return canAffordCoins && canAffordGems;
    }
    
    private void ProcessPayment(ShopItem item)
    {
        if (item.coinCost > 0)
        {
            FarmManager.Instance.UseResource(ResourceType.Coins, item.coinCost);
        }
        
        if (item.gemCost > 0)
        {
            FarmManager.Instance.UseResource(ResourceType.Gems, item.gemCost);
        }
        
        // Real money payments zouden hier via platform stores worden verwerkt
    }
    
    private void GiveItemToPlayer(ShopItem item)
    {
        switch (item.itemType)
        {
            case ShopItemType.CardPack:
                GiveCardPack(item);
                break;
                
            case ShopItemType.FarmItem:
                GiveFarmItem(item);
                break;
                
            case ShopItemType.Currency:
                GiveCurrency(item);
                break;
                
            case ShopItemType.Decoration:
                GiveDecoration(item);
                break;
                
            case ShopItemType.Boost:
                ApplyBoost(item);
                break;
        }
    }
    
    private void GiveCardPack(ShopItem item)
    {
        // Geef willekeurige kaarten
        for (int i = 0; i < item.quantity; i++)
        {
            // In een echte implementatie zou je hier kaarten geven
            // op basis van zeldzaamheid en beschikbare kaarten
            Debug.Log($"Received card from {item.itemName}");
        }
    }
    
    private void GiveFarmItem(ShopItem item)
    {
        // Voeg farm items toe aan inventaris
        // In een echte implementatie zou je dit toevoegen aan een inventaris systeem
        Debug.Log($"Received {item.quantity}x {item.itemName}");
    }
    
    private void GiveCurrency(ShopItem item)
    {
        if (item.itemId.Contains("coin"))
        {
            FarmManager.Instance.AddResource(ResourceType.Coins, item.quantity);
        }
        else if (item.itemId.Contains("gem"))
        {
            FarmManager.Instance.AddResource(ResourceType.Gems, item.quantity);
        }
    }
    
    private void GiveDecoration(ShopItem item)
    {
        // Voeg decoratie toe aan inventaris
        Debug.Log($"Received decoration: {item.itemName}");
    }
    
    private void ApplyBoost(ShopItem item)
    {
        // Pas boost toe
        if (item.itemId == "growth_boost")
        {
            // Start groeiversneller
            StartCoroutine(GrowthBoostCoroutine());
        }
    }
    
    private System.Collections.IEnumerator GrowthBoostCoroutine()
    {
        Debug.Log("Growth boost activated for 24 hours!");
        // In een echte implementatie zou je hier de groeisnelheid verhogen
        yield return new WaitForSeconds(86400f); // 24 uur
        Debug.Log("Growth boost expired.");
    }
    
    // Getters
    public List<ShopItem> GetAvailableItems() => availableItems;
    public List<ShopItem> GetDailyOffers() => dailyOffers;
    public float GetTimeUntilDailyReset() => dailyOfferResetTime - (Time.time - lastDailyOfferReset);
}

// ShopItem.cs
[System.Serializable]
public class ShopItem
{
    public string itemId;
    public string itemName;
    public string description;
    
    // Kosten
    public int coinCost;
    public int gemCost;
    public float realMoneyCost; // Voor echte geld aankopen
    
    // Item eigenschappen
    public ShopItemType itemType;
    public int quantity;
    public bool isDailyOffer = false;
    
    // Visueel
    public string iconPath;
}

public enum ShopItemType
{
    CardPack,
    FarmItem,
    Currency,
    Decoration,
    Boost
}

// 5. SOCIAL SYSTEM
// -------------------------------------------------------

// SocialManager.cs
using System.Collections.Generic;
using UnityEngine;

public class SocialManager : MonoBehaviour
{
    public static SocialManager Instance { get; private set; }
    
    // Player info
    public string PlayerName { get; private set; } = "NewFarmer";
    public int AvatarId { get; private set; } = 0;
    
    // Clan systeem
    public string CurrentClanName { get; private set; } = "";
    public bool IsInClan { get { return !string.IsNullOrEmpty(CurrentClanName); } }
    private List<ClanMember> clanMembers = new List<ClanMember>();
    
    // Vrienden systeem
    private List<Friend> friends = new List<Friend>();
    private List<string> pendingFriendRequests = new List<string>();
    
    // Chat en berichten
    private List<ChatMessage> clanChat = new List<ChatMessage>();
    private Dictionary<string, List<ChatMessage>> privateChats = new Dictionary<string, List<ChatMessage>>();
    
    // Events
    public delegate void ClanJoinedHandler(string clanName);
    public event ClanJoinedHandler OnClanJoined;
    
    public delegate void FriendAddedHandler(Friend friend);
    public event FriendAddedHandler OnFriendAdded;
    
    public delegate void MessageReceivedHandler(ChatMessage message);
    public event MessageReceivedHandler OnMessageReceived;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSocialSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeSocialSystem()
    {
        LoadSocialData();
        
        // Simuleer wat vrienden en clan leden voor demo
        CreateDemoSocialData();
    }
    
    private void LoadSocialData()
    {
        // In een echte implementatie zou je dit laden van DataManager
        GameData data = DataManager.Instance?.LoadGameData();
        
        if (data != null)
        {
            PlayerName = data.playerName ?? "NewFarmer";
            AvatarId = data.avatarId;
            CurrentClanName = data.clanName ?? "";
            
            if (data.friends != null)
            {
                foreach (string friendName in data.friends)
                {
                    friends.Add(new Friend { name = friendName, isOnline = UnityEngine.Random.value > 0.5f });
                }
            }
        }
    }
    
    private void CreateDemoSocialData()
    {
        // Demo vrienden
        if (friends.Count == 0)
        {
            friends.Add(new Friend { name = "FarmerJan", isOnline = true, trophies = 1500 });
            friends.Add(new Friend { name = "TuinKabouter", isOnline = false, trophies = 1200 });
            friends.Add(new Friend { name = "MelkMeester", isOnline = true, trophies = 1800 });
        }
        
        // Demo clan
        if (!IsInClan)
        {
            CurrentClanName = "Boerenbond";
            clanMembers.Add(new ClanMember { name = PlayerName, role = ClanRole.Leader, trophies = BattleManager.Instance?.Trophies ?? 0 });
            clanMembers.Add(new ClanMember { name = "VeldMaarschalk", role = ClanRole.CoLeader, trophies = 2500 });
            clanMembers.Add(new ClanMember { name = "OogstKoning", role = ClanRole.Elder, trophies = 2200 });
            clanMembers.Add(new ClanMember { name = "ZadenZaaier", role = ClanRole.Member, trophies = 1900 });
        }
    }
    
    // Player profiel
    public void SetPlayerName(string newName)
    {
        PlayerName = newName;
        // In een echte implementatie zou je dit opslaan
    }
    
    public void SetAvatar(int avatarId)
    {
        AvatarId = avatarId;
        // In een echte implementatie zou je dit opslaan
    }
    
    // Vrienden systeem
    public void SendFriendRequest(string friendName)
    {
        if (friendName == PlayerName) return;
        
        // Check of al vrienden zijn
        if (friends.Exists(f => f.name == friendName)) return;
        
        // In een echte implementatie zou je dit naar een server sturen
        Debug.Log($"Friend request sent to {friendName}");
    }
    
    public void AcceptFriendRequest(string friendName)
    {
        if (pendingFriendRequests.Contains(friendName))
        {
            pendingFriendRequests.Remove(friendName);
            
            Friend newFriend = new Friend
            {
                name = friendName,
                isOnline = UnityEngine.Random.value > 0.5f,
                trophies = UnityEngine.Random.Range(500, 3000)
            };
            
            friends.Add(newFriend);
            OnFriendAdded?.Invoke(newFriend);
            
            Debug.Log($"Added {friendName} as friend!");
        }
    }
    
    public void RemoveFriend(string friendName)
    {
        friends.RemoveAll(f => f.name == friendName);
        Debug.Log($"Removed {friendName} from friends list");
    }
    
    // Clan systeem
    public void JoinClan(string clanName)
    {
        if (IsInClan) return;
        
        CurrentClanName = clanName;
        
        // In een echte implementatie zou je clan data van server laden
        // Voor nu voegen we de speler toe als lid
        clanMembers.Add(new ClanMember
        {
            name = PlayerName,
            role = ClanRole.Member,
            trophies = BattleManager.Instance?.Trophies ?? 0
        });
        
        OnClanJoined?.Invoke(clanName);
        Debug.Log($"Joined clan: {clanName}");
    }
    
    public void LeaveClan()
    {
        if (!IsInClan) return;
        
        string oldClan = CurrentClanName;
        CurrentClanName = "";
        clanMembers.Clear();
        clanChat.Clear();
        
        Debug.Log($"Left clan: {oldClan}");
    }
    
    public void PromoteClanMember(string memberName, ClanRole newRole)
    {
        ClanMember member = clanMembers.Find(m => m.name == memberName);
        if (member != null)
        {
            member.role = newRole;
            Debug.Log($"Promoted {memberName} to {newRole}");
        }
    }
    
    public void KickClanMember(string memberName)
    {
        clanMembers.RemoveAll(m => m.name == memberName);
        Debug.Log($"Kicked {memberName} from clan");
    }
    
    // Chat systeem
    public void SendClanMessage(string message)
    {
        if (!IsInClan) return;
        
        ChatMessage chatMessage = new ChatMessage
        {
            senderName = PlayerName,
            message = message,
            timestamp = System.DateTime.Now,
            messageType = ChatMessageType.Clan
        };
        
        clanChat.Add(chatMessage);
        OnMessageReceived?.Invoke(chatMessage);
        
        Debug.Log($"[CLAN] {PlayerName}: {message}");
    }
    
    public void SendPrivateMessage(string recipientName, string message)
    {
        if (!privateChats.ContainsKey(recipientName))
        {
            privateChats[recipientName] = new List<ChatMessage>();
        }
        
        ChatMessage chatMessage = new ChatMessage
        {
            senderName = PlayerName,
            recipientName = recipientName,
            message = message,
            timestamp = System.DateTime.Now,
            messageType = ChatMessageType.Private
        };
        
        privateChats[recipientName].Add(chatMessage);
        OnMessageReceived?.Invoke(chatMessage);
        
        Debug.Log($"[PM to {recipientName}] {PlayerName}: {message}");
    }
    
    // Challenge systeem
    public void ChallengeFriend(string friendName)
    {
        Friend friend = friends.Find(f => f.name == friendName);
        if (friend != null && friend.isOnline)
        {
            Debug.Log($"Challenge sent to {friendName}!");
            // In een echte implementatie zou je een PvP match starten
        }
    }
    
    // Getters
    public List<Friend> GetFriends() => friends;
    public List<ClanMember> GetClanMembers() => clanMembers;
    public List<ChatMessage> GetClanChat() => clanChat;
    public List<ChatMessage> GetPrivateChat(string friendName)
    {
        if (privateChats.ContainsKey(friendName))
            return privateChats[friendName];
        return new List<ChatMessage>();
    }
}

// Social data classes
[System.Serializable]
public class Friend
{
    public string name;
    public bool isOnline;
    public int trophies;
    public int level;
    public System.DateTime lastSeen;
}

[System.Serializable]
public class ClanMember
{
    public string name;
    public ClanRole role;
    public int trophies;
    public int donationsGiven;
    public int donationsReceived;
    public System.DateTime lastSeen;
}

public enum ClanRole
{
    Member,
    Elder,
    CoLeader,
    Leader
}

[System.Serializable]
public class ChatMessage
{
    public string senderName;
    public string recipientName; // Voor private berichten
    public string message;
    public System.DateTime timestamp;
    public ChatMessageType messageType;
}

public enum ChatMessageType
{
    Clan,
    Private,
    System
}

// 6. UI CONTROLLERS
// -------------------------------------------------------

// UIManager.cs - Hoofdcontroller voor alle UI
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("Main UI Elements")]
    public GameObject farmUI;
    public GameObject battleUI;
    public GameObject shopUI;
    public GameObject socialUI;
    
    [Header("Top Bar")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI trophiesText;
    public TextMeshProUGUI levelText;
    public Slider experienceSlider;
    
    [Header("Bottom Navigation")]
    public Button farmButton;
    public Button battleButton;
    public Button shopButton;
    public Button socialButton;
    
    private GameState currentUIState = GameState.Farm;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeUI()
    {
        // Subscribe to events
        if (FarmManager.Instance != null)
        {
            FarmManager.Instance.OnCoinsChanged += UpdateCoinsDisplay;
            FarmManager.Instance.OnGemsChanged += UpdateGemsDisplay;
        }
        
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnTrophiesChanged += UpdateTrophiesDisplay;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerLevelChanged += UpdateLevelDisplay;
        }
        
        // Setup button listeners
        farmButton.onClick.AddListener(() => SwitchUIState(GameState.Farm));
        battleButton.onClick.AddListener(() => SwitchUIState(GameState.Battle));
        shopButton.onClick.AddListener(() => SwitchUIState(GameState.Shop));
        socialButton.onClick.AddListener(() => SwitchUIState(GameState.Social));
        
        // Initialize displays
        UpdateAllDisplays();
        SwitchUIState(GameState.Farm);
    }
    
    private void UpdateAllDisplays()
    {
        UpdateCoinsDisplay(FarmManager.Instance?.Coins ?? 0);
        UpdateGemsDisplay(FarmManager.Instance?.Gems ?? 0);
        UpdateTrophiesDisplay(BattleManager.Instance?.Trophies ?? 0);
        UpdateLevelDisplay(GameManager.Instance?.PlayerLevel ?? 1);
    }
    
    private void UpdateCoinsDisplay(int coins)
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }
    
    private void UpdateGemsDisplay(int gems)
    {
        if (gemsText != null)
            gemsText.text = gems.ToString();
    }
    
    private void UpdateTrophiesDisplay(int trophies)
    {
        if (trophiesText != null)
            trophiesText.text = trophies.ToString();
    }
    
    private void UpdateLevelDisplay(int level)
    {
        if (levelText != null)
            levelText.text = $"Level {level}";
            
        if (experienceSlider != null && GameManager.Instance != null)
        {
            experienceSlider.value = (float)GameManager.Instance.Experience / GameManager.Instance.ExperienceToNextLevel;
        }
    }
    
    public void SwitchUIState(GameState newState)
    {
        currentUIState = newState;
        
        // Hide all UI panels
        farmUI?.SetActive(false);
        battleUI?.SetActive(false);
        shopUI?.SetActive(false);
        socialUI?.SetActive(false);
        
        // Show the correct panel
        switch (newState)
        {
            case GameState.Farm:
                farmUI?.SetActive(true);
                break;
            case GameState.Battle:
                battleUI?.SetActive(true);
                break;
            case GameState.Shop:
                shopUI?.SetActive(true);
                break;
            case GameState.Social:
                socialUI?.SetActive(true);
                break;
        }
        
        // Update button states
        UpdateNavigationButtons();
    }
    
    private void UpdateNavigationButtons()
    {
        // Reset all button colors
        farmButton.GetComponent<Image>().color = currentUIState == GameState.Farm ? Color.green : Color.white;
        battleButton.GetComponent<Image>().color = currentUIState == GameState.Battle ? Color.red : Color.white;
        shopButton.GetComponent<Image>().color = currentUIState == GameState.Shop ? Color.blue : Color.white;
        socialButton.GetComponent<Image>().color = currentUIState == GameState.Social ? Color.yellow : Color.white;
    }
}

// FarmUIController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FarmUIController : MonoBehaviour
{
    [Header("Farm Info")]
    public TextMeshProUGUI farmLevelText;
    public TextMeshProUGUI farmItemsText;
    
    [Header("Resource Display")]
    public TextMeshProUGUI grainText;
    public TextMeshProUGUI vegetablesText;
    public TextMeshProUGUI fruitsText;
    public TextMeshProUGUI milkText;
    
    [Header("Farm Controls")]
    public Button plantButton;
    public Button harvestAllButton;
    public Button upgradeFarmButton;
    
    [Header("Item Selection")]
    public Dropdown itemDropdown;
    public Button placeItemButton;
    
    private string selectedItemName = "";
    
    private void Start()
    {
        InitializeFarmUI();
    }
    
    private void InitializeFarmUI()
    {
        // Subscribe to farm events
        if (FarmManager.Instance != null)
        {
            FarmManager.Instance.OnResourceChanged += UpdateResourceDisplay;
            FarmManager.Instance.OnFarmLevelChanged += UpdateFarmDisplay;
        }
        
        // Setup buttons
        plantButton.onClick.AddListener(OnPlantButtonClicked);
        harvestAllButton.onClick.AddListener(OnHarvestAllButtonClicked);
        upgradeFarmButton.onClick.AddListener(OnUpgradeFarmButtonClicked);
        placeItemButton.onClick.AddListener(OnPlaceItemButtonClicked);
        
        // Setup dropdown
        SetupItemDropdown();
        
        // Initial update
        UpdateAllDisplays();
    }
    
    private void SetupItemDropdown()
    {
        itemDropdown.options.Clear();
        
        // Add crop options
        itemDropdown.options.Add(new Dropdown.OptionData("Tarwe"));
        itemDropdown.options.Add(new Dropdown.OptionData("Maïs"));
        itemDropdown.options.Add(new Dropdown.OptionData("Wortelen"));
        
        // Add animal options
        itemDropdown.options.Add(new Dropdown.OptionData("Koe"));
        itemDropdown.options.Add(new Dropdown.OptionData("Kip"));
        itemDropdown.options.Add(new Dropdown.OptionData("Schaap"));
        
        itemDropdown.onValueChanged.AddListener(OnItemSelected);
        
        if (itemDropdown.options.Count > 0)
        {
            selectedItemName = itemDropdown.options[0].text;
        }
    }
    
    private void OnItemSelected(int index)
    {
        if (index >= 0 && index < itemDropdown.options.Count)
        {
            selectedItemName = itemDropdown.options[index].text;
        }
    }
    
    private void UpdateAllDisplays()
    {
        UpdateFarmDisplay(FarmManager.Instance?.FarmLevel ?? 1);
        UpdateResourceDisplay(ResourceType.Grain, FarmManager.Instance?.GetResourceAmount(ResourceType.Grain) ?? 0);
        UpdateResourceDisplay(ResourceType.Vegetables, FarmManager.Instance?.GetResourceAmount(ResourceType.Vegetables) ?? 0);
        UpdateResourceDisplay(ResourceType.Fruits, FarmManager.Instance?.GetResourceAmount(ResourceType.Fruits) ?? 0);
        UpdateResourceDisplay(ResourceType.Milk, FarmManager.Instance?.GetResourceAmount(ResourceType.Milk) ?? 0);
    }
    
    private void UpdateFarmDisplay(int farmLevel)
    {
        if (farmLevelText != null)
            farmLevelText.text = $"Farm Level: {farmLevel}";
            
        if (farmItemsText != null && FarmManager.Instance != null)
            farmItemsText.text = $"Items: {FarmManager.Instance.MaxFarmItems}";
    }
    
    private void UpdateResourceDisplay(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Grain:
                if (grainText != null) grainText.text = amount.ToString();
                break;
            case ResourceType.Vegetables:
                if (vegetablesText != null) vegetablesText.text = amount.ToString();
                break;
            case ResourceType.Fruits:
                if (fruitsText != null) fruitsText.text = amount.ToString();
                break;
            case ResourceType.Milk:
                if (milkText != null) milkText.text = amount.ToString();
                break;
        }
    }
    
    private void OnPlantButtonClicked()
    {
        Debug.Log("Plant mode activated - click on empty farm plot to plant");
        // In een echte implementatie zou je hier de plant modus activeren
    }
    
    private void OnHarvestAllButtonClicked()
    {
        Debug.Log("Harvesting all ready crops and animals");
        // In een echte implementatie zou je hier alle oogstbare items oogsten
    }
    
    private void OnUpgradeFarmButtonClicked()
    {
        if (FarmManager.Instance != null)
        {
            bool success = FarmManager.Instance.UpgradeFarm();
            if (success)
            {
                Debug.Log("Farm upgraded successfully!");
            }
            else
            {
                Debug.Log("Not enough coins to upgrade farm");
            }
        }
    }
    
    private void OnPlaceItemButtonClicked()
    {
        if (!string.IsNullOrEmpty(selectedItemName))
        {
            Debug.Log($"Place item mode activated for: {selectedItemName}");
            // In een echte implementatie zou je hier de plaatsing modus activeren
        }
    }
}

// BattleUIController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUIController : MonoBehaviour
{
    [Header("Battle Info")]
    public TextMeshProUGUI trophiesText;
    public TextMeshProUGUI arenaText;
    public Slider elixirSlider;
    public TextMeshProUGUI elixirText;
    
    [Header("Battle Controls")]
    public Button findMatchButton;
    public Button deckButton;
    public Button endBattleButton;
    
    [Header("Card Hand")]
    public Transform cardHandParent;
    public GameObject cardUIPrefab;
    
    [Header("Battle Status")]
    public TextMeshProUGUI battleTimeText;
    public TextMeshProUGUI playerTowersText;
    public TextMeshProUGUI enemyTowersText;
    
    private List<CardUI> cardUIElements = new List<CardUI>();
    
    private void Start()
    {
        InitializeBattleUI();
    }
    
    private void InitializeBattleUI()
    {
        // Subscribe to battle events
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnTrophiesChanged += UpdateTrophiesDisplay;
            BattleManager.Instance.OnArenaChanged += UpdateArenaDisplay;
            BattleManager.Instance.OnElixirChanged += UpdateElixirDisplay;
            BattleManager.Instance.OnBattleStateChanged += UpdateBattleState;
            BattleManager.Instance.OnBattleTimeChanged += UpdateBattleTime;
            BattleManager.Instance.OnTowerDestroyed += UpdateTowerDisplay;
        }
        
        // Setup buttons
        findMatchButton.onClick.AddListener(OnFindMatchClicked);
        deckButton.onClick.AddListener(OnDeckButtonClicked);
        endBattleButton.onClick.AddListener(OnEndBattleClicked);
        
        // Initial update
        UpdateAllDisplays();
        SetupCardHand();
    }
    
    private void UpdateAllDisplays()
    {
        UpdateTrophiesDisplay(BattleManager.Instance?.Trophies ?? 0);
        UpdateArenaDisplay(BattleManager.Instance?.ArenaLevel ?? 1);
        UpdateElixirDisplay(BattleManager.Instance?.GetCurrentElixir() ?? 0, 10);
        UpdateBattleState(BattleManager.Instance?.IsInBattle() ?? false);
    }
    
    private void UpdateTrophiesDisplay(int trophies)
    {
        if (trophiesText != null)
            trophiesText.text = $"Trophies: {trophies}";
    }
    
    private void UpdateArenaDisplay(int arena)
    {
        if (arenaText != null)
            arenaText.text = $"Arena {arena}";
    }
    
    private void UpdateElixirDisplay(float current, float max)
    {
        if (elixirSlider != null)
            elixirSlider.value = current / max;
            
        if (elixirText != null)
            elixirText.text = $"{current:F1}/{max}";
    }
    
    private void UpdateBattleState(bool inBattle)
    {
        findMatchButton.gameObject.SetActive(!inBattle);
        endBattleButton.gameObject.SetActive(inBattle);
        
        // Show/hide battle-specific UI
        if (battleTimeText != null)
            battleTimeText.gameObject.SetActive(inBattle);
        if (playerTowersText != null)
            playerTowersText.gameObject.SetActive(inBattle);
        if (enemyTowersText != null)
            enemyTowersText.gameObject.SetActive(inBattle);
    }
    
    private void UpdateBattleTime(float remainingTime)
    {
        if (battleTimeText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            battleTimeText.text = $"{minutes}:{seconds:00}";
        }
    }
    
    private void UpdateTowerDisplay(bool isPlayerTower, int remaining)
    {
        if (isPlayerTower && playerTowersText != null)
        {
            playerTowersText.text = $"Player Towers: {remaining}";
        }
        else if (!isPlayerTower && enemyTowersText != null)
        {
            enemyTowersText.text = $"Enemy Towers: {remaining}";
        }
    }
    
    private void SetupCardHand()
    {
        // Clear existing cards
        foreach (var cardUI in cardUIElements)
        {
            if (cardUI != null)
                Destroy(cardUI.gameObject);
        }
        cardUIElements.Clear();
        
        // Create new card UI elements
        if (BattleManager.Instance != null)
        {
            List<BattleCard> currentDeck = BattleManager.Instance.GetCurrentDeck();
            for (int i = 0; i < Mathf.Min(4, currentDeck.Count); i++) // Show first 4 cards in hand
            {
                GameObject cardObj = Instantiate(cardUIPrefab, cardHandParent);
                CardUI cardUI = cardObj.GetComponent<CardUI>();
                
                if (cardUI != null)
                {
                    cardUI.SetCard(currentDeck[i], i);
                    cardUI.OnCardClicked += OnCardClicked;
                    cardUIElements.Add(cardUI);
                }
            }
        }
    }
    
    private void OnCardClicked(int cardIndex)
    {
        Debug.Log($"Card {cardIndex} clicked - enter placement mode");
        // In een echte implementatie zou je hier de kaart plaatsing modus activeren
    }
    
    private void OnFindMatchClicked()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.StartBattle();
            Debug.Log("Finding match...");
        }
    }
    
    private void OnDeckButtonClicked()
    {
        Debug.Log("Opening deck management");
        // In een echte implementatie zou je hier het deck management scherm openen
    }
    
    private void OnEndBattleClicked()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.EndBattle(false);
            Debug.Log("Battle ended");
        }
    }
}

// CardUI.cs - UI component voor kaarten
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("Card Display")]
    public Image cardImage;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI healthText;
    public Button cardButton;
    
    private BattleCard card;
    private int cardIndex;
    
    public delegate void CardClickedHandler(int cardIndex);
    public event CardClickedHandler OnCardClicked;
    
    private void Start()
    {
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(() => OnCardClicked?.Invoke(cardIndex));
        }
    }
    
    public void SetCard(BattleCard newCard, int index)
    {
        card = newCard;
        cardIndex = index;
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (card == null) return;
        
        if (cardNameText != null)
            cardNameText.text = card.cardName;
            
        if (manaCostText != null)
            manaCostText.text = card.manaCost.ToString();
            
        if (damageText != null)
            damageText.text = card.damage.ToString();
            
        if (healthText != null)
            healthText.text = card.health.ToString();
            
        // Set card background color based on type
        if (cardImage != null)
        {
            switch (card.cardType)
            {
                case CardType.Troop:
                    cardImage.color = Color.red;
                    break;
                case CardType.Spell:
                    cardImage.color = Color.blue;
                    break;
                case CardType.Building:
                    cardImage.color = Color.gray;
                    break;
                case CardType.FarmAnimal:
                    cardImage.color = Color.green;
                    break;
            }
        }
    }
}

// 7. SETUP INSTRUCTIONS
// -------------------------------------------------------

/*
INSTALLATIE INSTRUCTIES VOOR UNITY:

1. UNITY PROJECT SETUP:
   - Maak een nieuw 2D Unity project
   - Importeer TextMeshPro package (Window > TextMeshPro > Import TMP Essential Resources)
   - Stel het project in voor Android build (File > Build Settings > Android)

2. SCENE SETUP:
   - Maak een nieuwe scene genaamd "MainScene"
   - Voeg een Canvas toe met Screen Space - Overlay
   - Voeg een EventSystem toe (GameObject > UI > Event System)

3. PREFAB CREATION:
   - Maak prefabs voor:
     * Crop items (met SpriteRenderer en Crop script)
     * Animal items (met SpriteRenderer en Animal script)
     * Card UI (met Image, TextMeshPro en CardUI script)
     * Farm tiles/grid system

4. UI SETUP:
   - Bouw de UI zoals getoond in de mockups
   - Verbind alle UI elementen met de juiste controllers
   - Voeg sprites toe voor kaarten, farm items, UI elementen

5. SCRIPTS TOEVOEGEN:
   - Kopieer alle scripts naar je Scripts folder
   - Maak empty GameObjects voor de managers en voeg scripts toe:
     * GameManager (met GameManager script)
     * FarmManager (met FarmManager script)
     * BattleManager (met BattleManager script)
     * ShopManager (met ShopManager script)
     * SocialManager (met SocialManager script)
     * UIManager (met UIManager script op Canvas)

6. TESTING:
   - Test elke functionaliteit stap voor stap
   - Begin met farm systeem, dan battle, dan shop en social
   - Gebruik Debug.Log statements om functionaliteit te verifiëren

7. ANDROID BUILD:
   - Configureer Android SDK en NDK
   - Set minimum API level naar 24 (Android 7.0)
   - Build en test op Android apparaat

DOWNLOAD INSTRUCTIES:
- Kopieer alle code naar .cs bestanden in je Unity project
- Organiseer in folders: Scripts/Managers, Scripts/UI, Scripts/Data
- Voeg sprites en assets toe voor complete gameplay ervaring

*/
