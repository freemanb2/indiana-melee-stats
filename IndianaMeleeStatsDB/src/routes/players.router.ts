import express, { json, Request, Response } from "express";
import { ConnectionClosedEvent, Int32, ObjectId } from "mongodb";
import { collections } from "../services/database.service";
import Player from "../models/player";
import Set from "../models/set";
import Tournament from "../models/tournament";
import Event from "../models/event";
import { sortBy, forEach } from "lodash";

export const playersRouter = express.Router();

const minimumTournamentsAttended = 8;

playersRouter.use(express.json());

playersRouter.get("/", async (req: Request, res: Response) => {
    try {
       const players = (await collections.players?.find({}).toArray()) as unknown as Player[];

        res.status(200).header("Access-Control-Allow-Origin", "*").send(players);
    } catch (error:any) {
        res.status(500).send(error.message);
    }
});

playersRouter.get("/rankings", async (req: Request, res: Response) => {
    try {
        const query = { Elo: { $gte: 0 } };
        const players = (await collections.players.find(query).toArray()) as unknown as Player[];

        var staleDate = new Date();
        staleDate.setDate(staleDate.getDate() - 365);
        var playersWithSufficientSets = new Array<Player>();
        await Promise.all(players.map(async (player) => {
            if (player.TournamentsAttended.length > minimumTournamentsAttended){
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

playersRouter.get("/:gamerTag/headToHeads", async (req: Request, res: Response) => {
    var gamerTag = req?.params?.gamerTag;

    try {
        var playerQuery = { "GamerTag": { $regex: new RegExp(`^${escapeRegExp(gamerTag)}$`), $options: "i" } };
        var player = (await collections.players.findOne(playerQuery)) as unknown as Player;

        const setsQuery = { "Players._id": player._id };
        var sets = (await collections.sets.find(setsQuery).toArray()) as unknown as Set[];

        var setCountsByOpponent = new Array<[string, string, number, number]>();
        await Promise.all(sets.map(async (set) => {
            var opponent = set.Players.find(p => p._id != player._id);

            // const tournamentsQuery = { "Events.Sets.Players._id": opponent._id };
            // var opponentTournaments = (await collections.tournaments.find(tournamentsQuery).toArray()) as unknown as Tournament[];
            // var opponentInactive = opponentTournaments.length < minimumTournamentsAttended;
            // if(opponentInactive) return;

            var win = Number(set.WinnerId == player._id);
            var loss = Number(!win);

            if(setCountsByOpponent.find(x => x[0] == opponent._id) == undefined){
                setCountsByOpponent.push([opponent._id, opponent.GamerTag, win, loss]);
            }
            else {
                setCountsByOpponent.find(x => x[0] == opponent._id)[2] = setCountsByOpponent.find(x => x[0] == opponent._id)[2] + win;
                setCountsByOpponent.find(x => x[0] == opponent._id)[3] = setCountsByOpponent.find(x => x[0] == opponent._id)[3] + loss;
            }
        }));

        setCountsByOpponent = sortBy(setCountsByOpponent, [setCount => -(setCount[2] + setCount[3])]);

        if (setCountsByOpponent) {
            res.status(200).header("Access-Control-Allow-Origin", "*").send(setCountsByOpponent);
        } else {
            console.log(`Error retrieving Head-to-Head stats for ${gamerTag}`);
            throw new Error();
        }
    } catch (error) {
        console.log(error);
        res.status(404).send(`Error retrieving Head-to-Head stats for ${gamerTag}`);
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
        const query = { GamerTag: { $regex: new RegExp(`^${gamerTag}$`), $options: "i" } };
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

function escapeRegExp(text: string) {
    return text.replace(/[-[\]{}()*+?.,\\^$|#\s]/g, '\\$&');
  }