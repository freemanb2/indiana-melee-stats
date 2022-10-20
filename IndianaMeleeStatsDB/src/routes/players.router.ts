import express, { Request, Response } from "express";
import { Int32, ObjectId } from "mongodb";
import { collections } from "../services/database.service";
import Player from "../models/player";
import Set from "../models/set";
import { sortBy } from "lodash";

export const playersRouter = express.Router();

playersRouter.use(express.json());

playersRouter.get("/", async (req: Request, res: Response) => {
    try {
       const players = (await collections.players?.find({}).toArray()) as unknown as Player[];

        res.status(200).header("Access-Control-Allow-Origin", "*").send(players);
    } catch (error:any) {
        res.status(500).send(error.message);
    }
});

playersRouter.get("/sets/:gamerTag", async (req: Request, res: Response) => {
    var gamerTag = req?.params?.gamerTag;

    try {
        const query = { "Players.GamerTag": { $regex: new RegExp(gamerTag), $options: "i" }, "Stale": false };
        const sets = (await collections.sets.find(query).toArray()) as unknown as Set[];

        if (sets) {
            res.status(200).header("Access-Control-Allow-Origin", "*").send(sets);
        }
    } catch (error) {
        res.status(404).send(`Unable to find matching document with id: ${req.params.id}`);
    }
});

playersRouter.get("/rankings", async (req: Request, res: Response) => {
    try {
        const query = { Elo: { $gte: 1000 } };
        const players = (await collections.players.find(query).toArray()) as unknown as Player[];

        var staleDate = new Date();
        staleDate.setDate(staleDate.getDate() - 180);
        var playersWithSufficientSets = new Array<Player>();
        await Promise.all(players.map(async (player) => {
            var tournamentCount = (await collections.tournaments.find({ "Events.Sets.Players.GamerTag": player.GamerTag, "Date": { $gte: staleDate } }).toArray()).length;
            if (tournamentCount >= 3){
                playersWithSufficientSets.push(player);
            }
        }));

        playersWithSufficientSets = sortBy(playersWithSufficientSets, [player => -player.Elo]);

        if (playersWithSufficientSets) {
            res.status(200).header("Access-Control-Allow-Origin", "*").send(playersWithSufficientSets);
        } else {
            throw new Error();
        }
    } catch (error:any) {
        res.status(500).send(error.message);
    }
});

playersRouter.get("/:id", async (req: Request, res: Response) => {
    const id = req?.params?.id;

    try {
        const query = { _id: id };
        const player = (await collections.players.findOne(query)) as unknown as Player;

        if (player) {
            res.status(200).header("Access-Control-Allow-Origin", "*").send(player);
        }
    } catch (error) {
        res.status(404).send(`Unable to find matching document with id: ${req.params.id}`);
    }
});

playersRouter.get("/gamertag/:gamerTag", async (req: Request, res: Response) => {
    var gamerTag = req?.params?.gamerTag;
    
    try {
        const query = { GamerTag: { $regex: new RegExp(gamerTag), $options: "i" } };
        const player = (await collections.players.findOne(query)) as unknown as Player;

        if (player) {
            res.status(200).header("Access-Control-Allow-Origin", "*").send(player);
        } else {
            throw new Error();
        }
    } catch (error) {
        res.status(404).send(`Unable to find matching player with GamerTag: ${req.params.gamerTag}`);
    }
});