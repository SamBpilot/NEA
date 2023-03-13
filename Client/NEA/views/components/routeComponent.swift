//
//  cardView.swift
//  NEA
//
//  Created by Sam Blewitt on 29/12/2022.
//

import SwiftUI

struct cardView: View {
    var route: routingSect
    
    var body: some View {
        VStack(alignment: .leading) {
            Text("Take the \(route.train) train for \(route.stops) stops")
                .font(.headline)
                .foregroundColor(.secondary)
            Text("From: \(route.getOn)")
                .font(.title)
                .fontWeight(.black)
                .foregroundColor(.primary)
                .lineLimit(3)
            Text("To: \(route.getOff)")
                .foregroundColor(.blue)
        }
    }
}

struct cardView_Previews: PreviewProvider {
    static var previews: some View {
        cardView(route: routingSect(train: "7", stops: 3, getOn: "7th street", getOff: "10th street"))
    }
}
