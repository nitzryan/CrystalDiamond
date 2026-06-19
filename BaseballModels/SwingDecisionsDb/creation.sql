CREATE TABLE SwingDecision (
	"GameId" INTEGER NOT NULL,
	"PitchId" INTEGER NOT NULL,

	"Year" INTEGER NOT NULL,
	"Month" INTEGER NOT NULL,

	"HitterId" INTEGER NOT NULL,
	"PitcherId" INTEGER NOT NULL,

	"LevelId" INTEGER NOT NULL,

	"PitchType" INTEGER NOT NULL,
	"CountBalls" INTEGER NOT NULL,
	"CountStrikes" INTEGER NOT NULL,
	"Outs" INTEGER NOT NULL,
	"BaseOccupancy" INTEGER NOT NULL,
	"DidSwing" INTEGER NOT NULL,

	"ProbSwing" REAL NOT NULL,
	"ValueSwing" REAL NOT NULL,
	"ValueNoSwing" REAL NOT NULL,
	"Value" REAL NOT NULL,

	PRIMARY KEY("GameId", "PitchId")
);

CREATE INDEX idx_SwingDecisionHitterDate ON SwingDecision
(
	"HitterId", "Year", "Month"
);

CREATE INDEX idx_SwingDecisionPitcherDate ON SwingDecision
(
	"PitcherId", "Year", "Month"
);


CREATE TABLE SwingResultAggregation
(
	"HitterId" INTEGER,
	"PitcherId" INTEGER,
	"LevelId" INTEGER NOT NULL,

	"Year" INTEGER NOT NULL,
	"Month" INTEGER,

	"PitchGroup" INTEGER NOT NULL,

	"NumSwings" INTEGER NOT NULL,
	"ValueSwings" REAL NOT NULL,
	"ValuePer100Swings" REAL NOT NULL,

	"NumNonSwings" INTEGER NOT NULL,
	"ValueNonSwings" REAL NOT NULL,
	"ValuePer100NonSwings" REAL NOT NULL,

	PRIMARY KEY("HitterId", "LevelId", "Year", "Month", "PitchGroup", "PitcherId")
);

CREATE INDEX idx_SwingResultAggregationPitcher ON SwingResultAggregation
(
	"PitcherId", "LevelId", "Year", "Month", "PitchGroup"
);