//
//  stationAutoComplete.swift
//  NEA
//
//  Created by Sam Blewitt on 04/03/2023.
//

import Foundation

/// Class containing auto complete options for user inputs.
class stationAutoComplete : ObservableObject {
    
    /// List of possible start stations
    @Published var startSationList: [station] = []

    /// List of possible end stations
    @Published var endStationList: [station] = []

    /// The name of the station the user would like to start at
    @Published var startName: String = ""

    /// The ID of the station the user would like to start at
    @Published var startID: Int = 0

    /// The name of the station the user would like to end at
    @Published var endName: String = ""

    /// The ID of the station the user would like to end at
    @Published var endID: Int = 0
    
    
    /// Set the final start station
    /// - Parameters:
    ///    - start: The station that the user would like to start at
    public func setStartStation(start: station)
    {
        startName = start.name
        startID = start.id
    }
    
    /// Set the final end station
    /// - Parameters:
    ///    - end: The station that the user would like to end at
    public func setEndStation(end: station)
    {
        endName = end.name
        endID = end.id
    }
    
    /// Set the list of possible start stations
    /// - Parameters:
    ///    - stations: List of possible stations for the user to choose frmo.
    public func setStartStations(stations: [station])
    {
        for halt in stations
        {
            if startSationList.contains(where: { $0.id == halt.id } )
            {
                // don't add it to the list because it's already there
            }
            else
            {
                startSationList.append(halt)
            }
        }
        while startSationList.count > 5
        {
            startSationList.remove(at: 0)
        }
    }
    
    /// Set the list of possible end stations
    /// - Parameters:
    ///    - stations: List of possible stations for the user to choose frmo.
    public func setEndStations(stations: [station])
    {
        for halt in stations
        {
            if endStationList.contains(where: { $0.id == halt.id } )
            {
                // don't add it to the list because it's already there
            }
            else
            {
                endStationList.append(halt)
            }
        }
        while endStationList.count > 5
        {
            endStationList.remove(at: 0)
        }
    }
}
