import express, { Request, Response } from "express";
import { Int32, ObjectId, FindCursor, WithId } from "mongodb";
import { collections } from "../services/database.service";
import Set from "../models/set";
import Event from "../models/event";
import Tournament from "../models/tournament";
import { forEach } from "lodash";

export const setsRouter = express.Router();

const minimumTournamentsAttended = 3;

setsRouter.use(express.json());

setsRouter.get("/:gamerTag", async (req: Request, res: Response) => {
    var gamerTag = req?.params?.gamerTag;

    try {
        const setsQuery = { "Players.GamerTag": { $regex: new RegExp(`^${gamerTag}$`), $options: "i" } };
        var sets = (await collections.sets.find(setsQuery).toArray()) as unknown as Set[];

        const tournamentsQuery = { "Events.Sets.Players.GamerTag": { $regex: new RegExp(`^${gamerTag}$`), $options: "i" } };
        var tournaments = (await collections.tournaments.find(tournamentsQuery).limit(20).sort({ "Date": -1 }).toArray()) as unknown as Tournament[];

        var setsByTournament = new Array;
        
        forEach(tournaments, (tournament) => {
            var allTournamentSets = new Array;
            forEach(tournament.Events, (event: Event) => {
                forEach(event.Sets, (set: Set) => {
                    allTournamentSets.push(set._id);
                });
            });

            var playerSetsInTournament = new Array;
            var participatedInTournament = false;
            forEach(sets, (set) => {
                if(allTournamentSets.includes(set._id)){
                    participatedInTournament = true;
                    playerSetsInTournament.push(set);
                }
            });

            if(participatedInTournament){
                setsByTournament.push({
                    tournamentId: tournament._id,
                    tournamentName: tournament.TournamentName,
                    tournamentLink: tournament.Link,
                    tournamentSets: playerSetsInTournament
                });
            }
        });

        if (setsByTournament) {
            res.status(200).header("Access-Control-Allow-Origin", "*").send(setsByTournament);
        }
    } catch (error) {
        res.status(404).send(`Unable to find matching document with id: ${req.params.id}`);
    }
});

setsRouter.get("/:id1/:id2", async (req: Request, res: Response) => {
    const id1 = req?.params?.id1;
    const id2 = req?.params?.id2;
    var setsArray = new Array<any>();
    try {
        const query = { $and: [ { "Players._id": id1 }, { "Players._id": id2 } ] };
        (await collections.sets.find(query)).forEach((set: any) => {
            setsArray.push(set);
        }).then(() => {
            if (setsArray) {
                res.status(200).header("Access-Control-Allow-Origin", "*").send(setsArray);
            }
        });
    } catch (error) {
        res.status(404).send(`Unable to find matching document with id: ${req.params.id}`);
    }
});

setsRouter.get("/:")