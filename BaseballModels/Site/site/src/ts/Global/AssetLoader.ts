const AssetPath = {
    Dates : '../../assets/dates.json.gz',
    Map : '../../assets/map.json.gz',
    PlayerSearch : '../../assets/player_search.json.gz',
    IconTraining : '/assets/training.svg',
    IconWarning : '/assets/warning.svg',
} as const;
type AssetPath = typeof AssetPath[keyof typeof AssetPath];

class AssetLoader {
    private static readonly CRITICAL_ASSETS: AssetPath[] = [
        AssetPath.Dates,
        AssetPath.Map,
    ];
    private static readonly DEFERRED_ASSETS: AssetPath[] = [
        AssetPath.PlayerSearch,
    ];
    private static readonly ICONS: AssetPath[] = [
        AssetPath.IconTraining,
        AssetPath.IconWarning,
    ];
    private readonly cache = new Map<AssetPath, Promise<JsonObject>>();
    private readonly iconMarkup = new Map<AssetPath, string>();

    /** Resolves once every critical asset has loaded; rejects if any critical load fails. */
    readonly ready: Promise<void>;

    readonly qualityLegend = new Map<string, QualityLegendEntry>();

    constructor() {
        this.ready = this.prefetch();
    }

    load<T extends JsonObject = JsonObject>(path: AssetPath): Promise<T> {
        let request = this.cache.get(path);
        if (request === undefined) {
            request = retrieveJson(path);
            this.cache.set(path, request);
        }
        return request as Promise<T>;
    }

    private prefetch(): Promise<void> {
        // Start every load now so deferred assets download in parallel.
        // The throwaway .catch prevents unhandled-rejection warnings before
        // a real awaiter attaches; the cached promise itself still rejects.
        for (const path of AssetLoader.DEFERRED_ASSETS) {
            this.load(path).catch(() => { /* surfaced later at the real await */ });
        }

        this.load(AssetPath.PlayerSearch).then(data => {
            searchBar = new SearchBar(data);
        });

        const orgMapReady = this.load(AssetPath.Map).then(data => {
            org_map = data; // TODO : Remove
            this.org_map = data
        });

        return Promise.all([
            ...AssetLoader.CRITICAL_ASSETS.map(path => this.load(path)),
            this.loadQualityLegend(),
            this.loadIcons(),
            orgMapReady, // That map has been written after being received
        ]).then(() => undefined);
    }

    //////////////// QUALITY LEGENDS /////////////////////
    private async loadQualityLegend(): Promise<void> {
        const response = await fetch('/qualityLegend');
        const rows = await response.json() as JsonArray;
        for (const r of rows) {
            const obj = r as JsonObject;
            const category = getJsonString(obj, 'category');
            const code = getJsonNumber(obj, 'code');
            this.qualityLegend.set(`${category}:${code}`, {
                label : getJsonString(obj, 'label'),
                blurb : getJsonString(obj, 'blurb'),
                severity : getJsonNumber(obj, 'severity'),
            });
        }
    }

    private async loadIcons(): Promise<void> {
        await Promise.all(AssetLoader.ICONS.map(async path => {
            const response = await fetch(path);
            this.iconMarkup.set(path, await response.text());
        }));
    }

    icon(path: AssetPath): string {
        const markup = this.iconMarkup.get(path);
        if (markup === undefined) {
            throw new Error(`Icon not loaded (awaited before ready?): ${path}`);
        }
        return markup;
    }

    ////////////////// Dates/Orgs ////////////////////////////
    async dates(): Promise<{ startYear: number; endYear: number; endMonth: number }> {
        const json = await this.load('../../assets/dates.json.gz');
        return {
            startYear : json['startYear'] as number,
            endYear   : json['endYear'] as number,
            endMonth  : json['endMonth'] as number,
        };
    }
    org_map : JsonObject | null = null
}
const assetLoader = new AssetLoader()

// Quality Rankings
type QualityLegendEntry = { label : string, blurb : string, severity : number }