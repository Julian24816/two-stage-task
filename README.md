# Gamification Of Decision Making Experiments

This repository holds the gamification framework that I developed for my bachelor thesis on the `main` branch.
Go to the `unity` branch for the whole unity project or download the android apk.

## Usage

`GeneralPurposeExperimentController` can controll any gamified decision making experiment out of the box. You just have to extend `ExperimentParameters` to provide the specific experiment parameters to the controller. 
Subscribe to the events in the experiment controller to controll your game and collect data from the experiment.

`TwoStageTask` holds an example implementation of the `ExperimentParameters` that is used in the unity project