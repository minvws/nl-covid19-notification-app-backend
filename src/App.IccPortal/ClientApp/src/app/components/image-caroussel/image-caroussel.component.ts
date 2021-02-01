// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { Component, HostListener, Input, OnInit } from '@angular/core';
import {CarousselImage} from '../../models/caroussel-image';

@Component({
    selector: 'app-image-caroussel',
    templateUrl: './image-caroussel.component.html',
    styleUrls: ['./image-caroussel.component.scss']
})
export class ImageCarousselComponent implements OnInit {
    @Input() images: Array<CarousselImage>;
    @Input() additionalStyle: string;

    canLeftScroll = false;
    canRightScroll = true;
    popupActive = false;

    @HostListener('document:keyup', ['$event'])
    handleKeyboardEvent(event: KeyboardEvent) {
        if (event.key === 'Escape') {
            this.popupActive = false;
        }
    }

    constructor() {
    }

    ngOnInit(): void {
    }

    bigImages() {
        return this.images.filter(i => !i.small);
    }
}
