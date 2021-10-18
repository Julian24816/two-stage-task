from os import walk
from os.path import join
from datetime import datetime


submitted_files_directory = "" # you have to fill this in



def getFileNames(): # see https://stackoverflow.com/a/3207973/5509966
    return next(walk(submitted_files_directory), (0, 0, []))[2]



def parseData(lines):
    return [parseLine(line) for line in lines[1:]] # ignore header line



def parseLine(line):
    columns = line[:-1].split(",") # ignore trailing \n
    return {
        "id": int(columns[0]), # trial number
        "is-complete": columns[1] == "yes", # completed
        "time-before": parseDatetime(columns[2]), # timestamp before black screen
        "duration-inter-trial": parseDuration(columns[3]), # Black Screen Duration
        "time-first-choice": parseDatetime(columns[4]), # Timestamp First Choice Shown
        "time-first-decision": parseDatetime(columns[5]), # Timestamp First Decision,
        "duration-first": parseDuration(columns[6]), # First Choice Reaction Time,
        "name-first": columns[7], # First Choice
        "is-common": columns[8] == "yes", # Common Transition,
        "name-second-stage": columns[9], # Second Stage,
        "time-second-choice": parseDatetime(columns[10]), # Timestamp Second Choice Shown,
        "time-second-decision": parseDatetime(columns[11]), # Timestamp Second Decision,
        "duration-second": parseDuration(columns[12]), # Second Decision Reaction Time,
        "name-second": columns[13], # Second Choice,
        "reward-probability": parseProbability(columns[14]), # Reward Probability,
        "is-win": columns[15] == "yes", # Reward,
        "time-end": parseDatetime(columns[16]), # Timestamp End
    }



def parseDatetime(string): # returns timestamp as datetime object
    if string == "": return None
    return datetime.strptime(string, '%m/%d/%Y %H:%M:%S')


def parseDuration(string): # returns duration in seconds
    if string == "": return None
    return float(string.split(":")[2])

def parseProbability(string):
    if string == "": return None
    return float(string[:-1]) / 100



evaluationHeadingRow = ["variation", "participant id", "complete trial count",
                        "start time", "duration",
                        "avg first reaction time", "avg second reaction time",
                        "avg reaction time", "sum of rewards", 
                        "number of correct choices", 
                        "filename"]
def evaluation(filename, data):
    separator = "-" if "-" in filename else "_"
    variation = filename.split(separator)[5]
    participant_id = filename.split(separator)[6]
    
    countComplete = 0
    sumReactionTimes = [0,0]
    numberOfWins = 0
    numberOfCorrectChoices = 0
    startTime = None
    endTime = None
    for row in data:
        if not row["is-complete"]: continue
        if startTime is None: startTime = row["time-before"]
        else: endTime = row["time-end"]
        countComplete += 1
        sumReactionTimes[0] += row["duration-first"]    
        sumReactionTimes[1] += row["duration-second"]
        if row["is-win"]: numberOfWins += 1
        if row["reward-probability"] > .5: numberOfCorrectChoices += 1

    avgReactionTimes = [0,0]
    avgReactionTime = 0
    if countComplete > 0:
        avgReactionTimes = [s / countComplete for s in sumReactionTimes]
        avgReactionTime = sum(avgReactionTimes) / 2

    return (
        variation,
        participant_id,
        countComplete,
        startTime,
        None if endTime is None else endTime - startTime,
        avgReactionTimes[0],
        avgReactionTimes[1],
        avgReactionTime,
        numberOfWins,
        numberOfCorrectChoices,
        filename
    )




def printAll(separator = ";", lineSep = "\n"):
    print(*evaluationHeadingRow, sep=separator)
    for filename in getFileNames():
        with open(join(submitted_files_directory, filename)) as file:
            data = parseData(file.readlines())
            print(*evaluation(filename, data), sep=separator, end=lineSep)


if __name__=="__main__":
    printAll()
