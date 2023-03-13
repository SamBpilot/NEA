//
//  RouteView.swift
//  NEA
//
//  Created by Sam Blewitt on 12/09/2022.
//

import SwiftUI

struct RouteView: View {
    @Binding public var start: String
    @Binding public var end : String
    @Binding public var stops: Array<routingSect>
    
    var body: some View {
        
        VStack{
            Text("Your route \(start) -> \(end)")
                .font(.title)
                .bold()
                .multilineTextAlignment(.center)
            Image("Map")
                .resizable()
                .padding()
                .scaledToFit()
            Text("Changes")
                .font(.title3)
            
            List{
                VStack(alignment: .leading) {
                    Label("Start at: \(start)",systemImage: "play")
                            .font(.title)
                            .foregroundColor(.black)
                            .lineLimit(3)
                    
                    Divider()
                    
                    ForEach(0..<stops.count, id:\.self) { i in
                        Spacer()
                        cardView(route:stops[i])
                        Divider()
                        Spacer()
                    }
                    

                    Label("Arrive: \(end)", systemImage: "stop.circle")
                        .font(.title)
                        .foregroundColor(.black)
                        .lineLimit(3)
                }
            }
        }
    }
}

//struct RouteView_Previews: PreviewProvider {
//    @State static var start = "test"
//    @State static var end = "test 2"
//    @State static var route : Array<routingSect> = [routingSect(train: "7", stops: 3, getOn: "test", getOff:"second station"), routingSect(train: "9", stops: 1, getOn: "second station", getOff: "test 2")]
//    
//    static var previews: some View {
////        RouteView(start: $start , end: $end, stops: $route)
//    }
//}
