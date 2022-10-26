import express, { json, Request, Response } from "express";
import { Int32, ObjectId } from "mongodb";
import { collections } from "../services/database.service";
import Player from "../models/player";
import Set from "../models/set";
import Tournament from "../models/tournament";
import Event from "../models/event";
import { sortBy, forEach } from "lodash";

export const playersRouter = express.Router();

const minimumTournamentsAttended = 12;

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
            var tournamentCount = (await collections.tournaments.find({ "Events.Sets.Players.GamerTag": player.GamerTag }).toArray()).length;
            if (tournamentCount >= minimumTournamentsAttended){
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

        const setsQuery = { "Players.GamerTag": { $regex: new RegExp(`^${player.GamerTag}$`), $options: "i" } };
        var sets = (await collections.sets.find(setsQuery).toArray()) as unknown as Set[];

        var setCountsByOpponent = new Array<[string, number, number]>();
        await Promise.all(sets.map(async (set) => {
            var opponent = set.Players.find(p => p.GamerTag.toLowerCase() != player.GamerTag.toLowerCase());

            const tournamentsQuery = { "Events.Sets.Players.GamerTag": { $regex: new RegExp(`^${escapeRegExp(opponent.GamerTag)}$`), $options: "i" } };
            var opponentTournaments = (await collections.tournaments.find(tournamentsQuery).toArray()) as unknown as Tournament[];
            var opponentInactive = opponentTournaments.length < minimumTournamentsAttended;
            if(opponentInactive) return;

            var winner;

            if(set.PlayerIds[0] == set.WinnerId){
                winner = set.Players[0];
            }
            else {
                winner = set.Players[1];
            }

            var won = winner.GamerTag.toLowerCase() == player.GamerTag.toLowerCase();

            if(setCountsByOpponent.find(x => x[0].toLowerCase() == opponent.GamerTag.toLowerCase()) == undefined){
                setCountsByOpponent.push([opponent.GamerTag, Number(won), Number(!won)]);
            }
            else {
                setCountsByOpponent.find(x => x[0].toLowerCase() == opponent.GamerTag.toLowerCase())[1] = setCountsByOpponent.find(x => x[0].toLowerCase() == opponent.GamerTag.toLowerCase())[1] + Number(won);
                setCountsByOpponent.find(x => x[0].toLowerCase() == opponent.GamerTag.toLowerCase())[2] = setCountsByOpponent.find(x => x[0].toLowerCase() == opponent.GamerTag.toLowerCase())[2] + Number(!won);
            }
        }));

        setCountsByOpponent = sortBy(setCountsByOpponent, [setCount => -(setCount[1] + setCount[2])]);

        if (setCountsByOpponent) {
            res.status(200).header("Access-Control-Allow-Origin", "*").send(setCountsByOpponent);
        } else {
            console.log("error");
            throw new Error();
        }
    } catch (error) {
        console.log(error);
        res.status(404).send(`Unable to find matching document with id: ${req.params.id}`);
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