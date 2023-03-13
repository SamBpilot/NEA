//
//  MainView.swift
//  NEA
//
//  Created by Sam Blewitt on 10/09/2022.
//

import SwiftUI
import Combine

struct MainView: View {
    @EnvironmentObject var server : ServerObject

    @State private var errorMessage: String = "An unkown error occured. Please contact your system administrator for more."
    @State private var isShowingAlert: Bool = false
    @State private var route = [routingSect]()
    @State private var isSegueing: Bool = false
    @StateObject private var viewModel = stationAutoComplete()
    
    
    var body: some View {
        NavigationView
        {
                VStack{
                    Text("Where would you like to go?")
                        .font(.title)
                    TextField("Start station", text: $viewModel.startName)
                        .textFieldStyle(RoundedBorderTextFieldStyle())
                        .onChange(of: viewModel.startName){ start in // https://stackoverflow.com/questions/67875966/why-is-the-method-onreceive-being-called-twice-in-this-piece-of-code
                            do {
                                if start == "" {
                                    throw NEA.cancelled
                                }
                                let SBW = try self.server.server.stationBeginsWith(name: start)
                                viewModel.setStartStations(stations: SBW)
                            } catch
                            {
                                // don't do anything
                            }
                        }

                List(viewModel.startSationList) { (stop: station) in
                    if (viewModel.startName == "" || stop.id == -1)
                    {
                        // don't do anything
                    }
                    else
                    {
                        Button( action: {
                            viewModel.setStartStation(start: stop)
                        }) {
                            HStack {
                                Text(stop.name)
                                Spacer()
                                ForEach(0..<stop.lines.count, id:\.self) { i in
                                    Text(stop.lines[i])
                                        .frame(alignment: .trailing)
                                    
                                }
                            }
                        }
                    }
                }
                
                TextField("Destination station", text: $viewModel.endName)
                    .textFieldStyle(RoundedBorderTextFieldStyle())
                    .onChange(of: viewModel.endName){ end in
                        do {
                            if end == "" {
                                throw NEA.cancelled
                            }
                            let Stations = try self.server.server.stationBeginsWith(name: end)
                            viewModel.setEndStations(stations: Stations)
                            
                        } catch
                        {
                            // don't do anything
                        }
                    }
                
                
                List(viewModel.endStationList) { (halt: station) in
                    if (viewModel.endName == "" || halt.id == -1)
                    {
                        // don't do anything
                    }
                    else
                    {
                        Button( action: {
                            viewModel.setEndStation(end: halt)
                        }) {
                            HStack {
                                Text(halt.name)
                                Spacer()
                                ForEach(0..<halt.lines.count, id:\.self) { i in
                                    Text(halt.lines[i])
                                        .frame(alignment: .trailing)
                                    
                                }
                            }
                        }
                    }
                }

                
                NavigationLink(destination: RouteView(start: $viewModel.startName, end: $viewModel.endName, stops: $route), isActive: $isSegueing) {
                    Label("Route me",systemImage: "arrow.right.to.line")
                        .padding()
                        .background(Color.gray)
                        .foregroundColor(.white)
                        .clipShape(RoundedRectangle(cornerRadius: 40))
                }
                .simultaneousGesture(TapGesture().onEnded({
                    
                    do{
                        print("Preparing to get a route")
                        if (self.viewModel.startName == "" || self.viewModel.endName == "") { throw NEA.cancelled} // if either are empty
                        
                        self.route = try self.server.server.getRoute(start: self.viewModel.startID, end: self.viewModel.endID)
                        self.isSegueing = true
                    }
                    catch NEA.invalidStation
                    {
                        self.errorMessage = NEA.invalidStation.description
                        self.isShowingAlert.toggle()
                    }
                    catch {
                        // something went wrong so send an alert
                        self.isShowingAlert.toggle()
                    }
                }))
            }
            .alert(errorMessage, isPresented: $isShowingAlert){
                Button("OK", role: .cancel) {
                    
                }
            }
        }
    }
}

struct MainView_Previews: PreviewProvider {
    static var previews: some View {
        MainView()
    }
}
